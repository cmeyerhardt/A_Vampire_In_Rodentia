using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public enum PlayerState { Idle, /*Walking, Jumping,*/Dashing, Feeding, CommenceFeeding, Dead, Hiding, Interacting/*, Flying*/}
//public enum CursorType { None, UI, Victim, Guard, Hide, PlayerDead };
//[System.Serializable]
//public class CursorMapping
//{
//    public CursorType type = CursorType.None;
//    public Texture2D texture = null;
//    [HideInInspector] public Vector2 hotspot = Vector2.zero;
//}


public class PlayerController : Character
{
    public FloatEvent makeSoundEvent = new FloatEvent();

    [Header("Player--")]
    public PlayerState playerState = PlayerState.Idle;
    public bool isHidden = false;

    [Header("Input")]
    [SerializeField] KeyCode sprintingKey = KeyCode.LeftShift;
    [SerializeField] KeyCode dashKey = KeyCode.Alpha1;
    //public bool allowFlying = true;
    //[SerializeField] [Range(10f, 30f)] float flyingSpeed = 10f;
    public bool allowKeyBoardTurn = true;
    
    [Header("Jumping")]
    [SerializeField] [Range(1f, 500f)] float jumpForce = 250f;
    [SerializeField] [Range(1f, 50f)] float airForward = 25f;
    [SerializeField] float maxJumpsAllowed = 2;
    [SerializeField] float diminishedUpForce = .85f;
    bool jump = false;
    int jumpCounter = 0;
    float currentUpForce;
    float currentJumpsAllowed = 0;

    [Header("Stunning")]
    [SerializeField] float stunRange = 3f;
    [SerializeField] float stunDuration = 3f;

    [Header("Noise")]
    [SerializeField] float sprintingNoiseLevel = 3f;
    [SerializeField] float walkingNoiseLevel = 0.1f;
    //[SerializeField] GameObject soundEffectFX = null;


    [Header("Stamina Drain")]
    public float walkStaminaDrain = 0f;
    public float sprintStaminaDrain = 4f;
    public float blinkStaminaDrain = 6f;
    [SerializeField] float dashCost = 30f;
    [SerializeField] float stunCost = 30f;



    // Cache
    Transform cameraPivot = null;
    Vector3 lastCollisionPoint = Vector3.zero;
    Vector3 direction = new Vector3();
    FeedingVictim currentVictim = null;
    [HideInInspector] public Feeder feeder = null;
    Health health = null;

    //[Header("Cursor")]
    //[SerializeField] CursorMapping[] cursorMappings = null;

    private new void Awake()
    {
        base.Awake();
        GetComponent<MovementTransform>().enabled = false;
        cameraPivot = transform.Find("CameraPivot");
        feeder = GetComponent<Feeder>();
        health = GetComponent<Health>();

        currentUpForce = diminishedUpForce;
        currentJumpsAllowed = maxJumpsAllowed;
        jumpCounter = 0;
        //ResetState();

        //makeSoundEvent.AddListener(MakeSoundEffect);
        //SetCursor(CursorType.None);
    }

    //private void MakeSoundEffect(float intensity)
    //{
    //    Destroy(Instantiate(soundEffectFX, transform.position, Quaternion.identity, null), .2f);
    //}

    private new void Start()
    {
        base.Start();
    }

    private void ResetState()
    {
        ChangeState(PlayerState.Idle);
    }

    public new void Update()
    {
        base.Update();
        // Player cannot control character while "latching on" to NPC or Dashing
        if (playerState == PlayerState.CommenceFeeding) { return; }
        if (playerState == PlayerState.Dashing) { return; }
        if (isStunned) { return; }

        ProcessMovementInput(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));

        if (Input.GetKeyDown(dashKey))
        {
            if (stamina.GetStaminaValue() > dashCost)
            {
                //Dash forward
                StartCoroutine(Dash());
            }
            //else
            //{
            //    textSpawner.SpawnText("Too Tired!", Color.green);
            //}
        }

        //ProcessRaycast();
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(objectInHand != null)
            {
                DropObject();
            }
        }
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Debug: Last Collision Point");
            navMeshAgent.enabled = false;
            transform.position = lastCollisionPoint;
            navMeshAgent.enabled = true;
        }
#endif
    }

    private IEnumerator Dash()
    {
        playerState = PlayerState.Dashing;
        textSpawner.SpawnText("Dashing!", Color.green);
        stamina.ModifyStamina(-dashCost);

        NavMeshHit hit;
        NavMesh.Raycast(transform.position, transform.position + 5f * transform.forward, out hit, 1);

        navMeshAgent.velocity = DetermineDirectionOfMovement(10f, 0f);// (new Vector3(0f, 0f, 2f);
        //navMeshAgent.SetDestination(hit.position);

        //float moveSpeed = navMeshAgent.speed;
        //navMeshAgent.speed = 50f;
        //navMeshAgent.SetDestination(transform.position + transform.forward * 5);
        //while ((transform.position - navMeshAgent.destination).magnitude > navMeshAgent.stoppingDistance)
        //{
        //    yield return null;
        //}
        //navMeshAgent.speed = moveSpeed;

        //navMeshAgent.Warp(transform.position + transform.forward * 10);


        yield return new WaitForSeconds(2f);
        playerState = PlayerState.Idle;
        yield return null;
    }



    /***********************
    * ABILITY
    ***********************/
    public override float GetDeltaModifier()
    {
        float modifier = 0f;
        if (Input.GetKey(sprintingKey))
        {
            modifier += 3f;
        }
        return modifier;
    }

    public bool CheckStunConditions(Protector guard)
    {
        if (guard != null)
        {
            if (IsInRange(guard.transform.position, stunRange))
            {
                if (stamina.GetStaminaValue() > stunCost)
                {
                    //Stun target
                    textSpawner.SpawnText("Stunning!", Color.green);
                    stamina.ModifyStamina(-stunCost);
                    guard.Stun(stunDuration);
                    return true;
                }
                else
                {
                    textSpawner.SpawnText("Too Tired!", Color.green);
                    return false;
                }
            }
            else
            {
                textSpawner.SpawnText("Out of Range!", Color.red);
                return false;
            }
        }
        else
        {
            textSpawner.SpawnText("No Target!", Color.red);
            return false;
        }
    }


    /***********************
    * FEEDING
    ***********************/

    public bool CheckFeedingConditions(FeedingVictim victim)
    {
        if (playerState != PlayerState.Feeding && victim != null && !victim.GetComponent<Character>().isDead)
        {
            if (IsInRange(victim.transform.position, GetStoppingDistance()))
            {
                StartCoroutine(CommenceFeeding(victim));
                return true;
            }
            else
            {
                textSpawner.SpawnText("Out of Range!", Color.red);
                return false;
            }
        }
        return false;
    }

    public IEnumerator CommenceFeeding(FeedingVictim victim)
    {
        playerState = PlayerState.CommenceFeeding;

        navMeshAgent.isStopped = true;
        currentVictim = victim;

        bool successful;
        victim.BeginFeeding(out successful);
        if (successful)
        {
            // Wait for Movement input to ramp down
            while (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
            {
                yield return null;
            }
            currentVictim.GetComponent<Character>().Stun(-1);
            feeder.AssignVictim(currentVictim);
            playerState = PlayerState.Feeding;
        }
        else
        {
            textSpawner.SpawnText("Failed to Feed");
            //todo -- NPC becomes alert/flees?
            playerState = PlayerState.Idle;
        }
    }

    /***********************
    * STATE
    ***********************/

    private void ChangeState(PlayerState newState)
    {
        playerState = newState;
        switch(playerState)
        {
            case PlayerState.Idle:
                currentJumpsAllowed = maxJumpsAllowed;
                currentUpForce = diminishedUpForce;
                break;
            case PlayerState.CommenceFeeding:
                break;
            case PlayerState.Dashing:
                break;
            case PlayerState.Feeding:
                break;
            case PlayerState.Hiding:
                textSpawner.SpawnText("-hidden-", Color.white);
                break;
            //case PlayerState.Flying:
            //    maxJumpsAllowed = Mathf.Infinity;
            //    currentUpForce = 1f;
            //    textSpawner.SpawnText("*Flying*", Color.cyan);
            //    break;
        }
    }

    public new void Die()
    {
        base.Die();
        textSpawner.SpawnText("I'm BLLUUUHHH.. dead", Color.red);
        //todo -- game over stuff
    }

    /***********************
    * MOVEMENT
    ***********************/

    private void ProcessMovementInput(float verticalMag, float horizontalMag)
    {
        direction = DetermineDirectionOfMovement(verticalMag, horizontalMag);

        if(allowKeyBoardTurn)
        {
            // Process Rotation
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Input.GetAxis("Rotation") * Vector3.up, turnSpeed * Time.deltaTime);
            }
        }

        // Process Translation
        if (horizontalMag != 0f || verticalMag != 0f)
        {
            if (currentInteractiable != null)
            {
                currentInteractiable.CancelInteract();
                textSpawner.SpawnText("-Cancel Interact-", Color.yellow);
                playerState = PlayerState.Idle;
            }
            if(playerState == PlayerState.Feeding)
            {
                textSpawner.SpawnText("-Cancel Feeding-", Color.yellow);
                feeder.CancelFeeding();
                playerState = PlayerState.Idle;
            }
            if (jump) // air controls
            {
                //todo should not have to break this up into conditions. find a way to limit velocity while in air
                if (rigidBody.velocity.y > 0) // going up
                {
                    rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity + direction * airForward * Time.deltaTime, sprintSpeed);
                }
                else // going down
                {
                    rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity + direction * Time.deltaTime, baseMovementSpeed);
                }
            }
            else // ground controls
            {
                float staminaCost = walkStaminaDrain;
                // Determine how fast the player moves: running, walking speed
                float movementSpeed = baseMovementSpeed;
                float noiseLevel = walkingNoiseLevel;
                if(Input.GetKey(sprintingKey))
                {
                    noiseLevel = sprintingNoiseLevel;
                    staminaCost = sprintStaminaDrain;
                    movementSpeed = sprintSpeed;
                }

                // The player model object turns toward the direction of movement
                model.forward = Vector3.Slerp(model.transform.forward, direction, turnSpeed * Time.deltaTime);

                // The object moves via navmesh in the direction of movement
                navMeshAgent.Move(direction * movementSpeed * Time.deltaTime);
                stamina.ModifyStaminaShowOnIncrement(-staminaCost * Time.deltaTime);
                makeSoundEvent.Invoke(noiseLevel);
                //Update animator
                //animator.SetFloat("ForwardSpeed", movementSpeed);
            }
        }
        // Process Jumping
        if (jumpCounter < maxJumpsAllowed)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump((jump) ? diminishedUpForce : 1f);
            }
            if (Input.GetButton("Jump"))
            {
                if (!jump) { Jump(1f); }
            }
        }
    }

    public Vector3 DetermineDirectionOfMovement(float verticalMag, float horizontalMag)
    {
        // Camera's directional vectors:
        Vector3 _cameraForward = cameraPivot.forward;
        Vector3 _cameraRight = cameraPivot.right;

        // Remove the y-directional change
        _cameraForward.y = 0f;
        _cameraRight.y = 0f;

        // Normalize direction vectors
        _cameraForward.Normalize();
        _cameraRight.Normalize();

        // Adjust the camera's normalized directional vectors by the magnitude of input
        return _cameraForward * verticalMag + _cameraRight * horizontalMag;
    }

    /***********************
     * JUMPING
     ***********************/
    private void Jump(float jumpScalar)
    {
        jumpCounter++;
        if(navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;

        if (direction == Vector3.zero)
        {
            rigidBody.AddForce(Vector3.up * jumpForce * jumpScalar * Input.GetAxis("Jump"));
        }
        else
        {
            rigidBody.AddForce((Vector3.up + direction) * jumpForce * jumpScalar * Input.GetAxis("Jump"));
        }
        jump = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (jump)
        {
            if ((collision.gameObject.tag == "Ground" || collision.gameObject.isStatic))
            {
                jump = false;
                lastCollisionPoint = transform.position;
                GetComponent<MovementTransform>().enabled = false;
                print(collision.gameObject.name + " Static: " + collision.gameObject.isStatic);
                print("Collided with ground");

                rigidBody.isKinematic = true;
                rigidBody.useGravity = false;
                navMeshAgent.enabled = true;
                navMeshAgent.isStopped = false;
                jumpCounter = 0;
            }
            else
            {
                if (!collision.collider.isTrigger)
                {
                    GetComponent<MovementTransform>().enabled = true;
                    print("Landed on object that is not navmesh/static");
                }
            }
        }
    }


    ///***********************
    //* RAYCASTING
    //***********************/

    //private void ProcessRaycast()
    //{
    //    if (InteractWithUI()) { return; }
    //    if (isDead)
    //    {
    //        SetCursor(CursorType.PlayerDead);
    //        return;
    //    }
    //    if (InteractWithComponent()) { return; }
    //    SetCursor(CursorType.None);
    //}

    //private bool InteractWithUI()
    //{
    //    if (EventSystem.current.IsPointerOverGameObject()) //is the cursor over UI?
    //    {
    //        SetCursor(CursorType.UI);
    //        return true;
    //    }
    //    return false;
    //}

    //private bool InteractWithComponent()
    //{
    //    RaycastHit[] hits = SortRaycasts();

    //    foreach (RaycastHit hit in hits)
    //    {
    //        IRaycast[] raycastables = hit.transform.GetComponents<IRaycast>();
    //        foreach (IRaycast raycastable in raycastables)
    //        {
    //            if (raycastable.HandleRaycast(this))
    //            {
    //                SetCursor(raycastable.GetCursorType());
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //RaycastHit[] SortRaycasts()
    //{
    //    //get all hits
    //    RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());

    //    float[] distances = new float[hits.Length];
    //    for (int i = 0; i < hits.Length; i++)
    //    {
    //        distances[i] = hits[i].distance;
    //    }
    //    Array.Sort(distances, hits);

    //    //sort array of hits
    //    //return
    //    return Physics.RaycastAll(GetMouseRay());
    //}

    //private static Ray GetMouseRay()
    //{
    //    return Camera.main.ScreenPointToRay(Input.mousePosition);
    //}

    ///***********************
    //* CURSOR
    //***********************/

    //private void SetCursor(CursorType type)
    //{
    //    CursorMapping mapping = GetCursorMapping(type);
    //    Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
    //}

    //private CursorMapping GetCursorMapping(CursorType type)
    //{
    //    foreach (CursorMapping mapping in cursorMappings)
    //    {
    //        if (mapping.type == type)
    //        {
    //            return mapping;
    //        }
    //    }
    //    return cursorMappings[0];
    //}

}
