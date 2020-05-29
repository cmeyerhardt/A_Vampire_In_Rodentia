using System.Collections;
using UnityEngine;

public enum PlayerState { Idle, /*Walking, Jumping,*/Dashing, Feeding, CommenceFeeding, Dead, Hiding, Interacting/*, Flying*/}
//public enum CursorType { None, UI, Victim, Guard, Hide, PlayerDead };
//[System.Serializable]
//public class CursorMapping
//{
//    public CursorType type = CursorType.None;
//    public Texture2D texture = null;
//    [HideInInspector] public Vector2 hotspot = Vector2.zero;
//}

//Animation States
//PlayerIdle = 0
//PlayerWalk = 1
//PlayerRun = 2
//PlayerAttack = 3
//PlayerFeed = 4
//Stunned = 5
//PlayerHide = 6


public class PlayerController : Character
{
    [Header("Player--")]
    public PlayerState playerState = PlayerState.Idle;
    public PlayerState lastState = PlayerState.Idle;
    public bool isHidden = false;
    public GameObject target = null;
    
    [Header("Input")]
    [SerializeField] KeyCode sprintingKey = KeyCode.LeftShift;
    [SerializeField] KeyCode dashKey = KeyCode.Space;
    public bool allowKeyBoardTurn = true;
    //public bool allowFlying = true;
    //[SerializeField] [Range(10f, 30f)] float flyingSpeed = 10f;


    [Header("Movement")]
    [Tooltip("The unit's actual speed.\n\nactualMovementSpeed = baseMovementSpeed + sprintSpeed * stamina% * sprintModifier")]
    public float actualMovementSpeed = 0f;

    [SerializeField] [Tooltip("The base amount of speed to add when sprinting.")]
    [Range(0f, 20f)] public float sprintSpeed = 10f;
    [SerializeField] [Range(1f, 10f)] public float dashDistance = 5f;
    [SerializeField] [Range(45f, 270f)] public float turnSpeed = 100f;

    [Header("Noise")]
    public FloatEvent makeSoundEvent = new FloatEvent();
    public float actualMovingNoiseLevel = 0f;
    [SerializeField] [Range(0,20f)] float walkingNoiseLevel = 0f;
    [SerializeField] [Range(0,20f)] float sprintingNoiseLevel = 3f;
    [SerializeField] [Range(0,20f)] float maxNoiseLevel = 6f;

    public bool walkLouderIfLowStamina = false;
    [SerializeField] AudioClip hunterStun = null;
    [SerializeField] [Range(0f, 1f)] public float hunterStunVolume = 1f;
    [SerializeField] AudioClip guardStun = null;
    [SerializeField] [Range(0f, 1f)] public float guardStunVolume = 1f;
    [SerializeField] AudioClip sprintFootsteps = null;
    [SerializeField] [Range(0f, 1f)] public float sprintFootStepsVolume = 1f;
    [SerializeField] [Range(0,1f)] float sprintFootstepsInterval = 1f;
    //[SerializeField] GameObject soundEffectFX = null;

    [Header("Modified by Stamina Levels")]
    [SerializeField]
    [Tooltip("How much will stamina levels affect sprint speed?\n\nEx. If Stamina is at 50% and sprintSpeedModifier is at 1, sprintSpeed will be modified by 100% of stamina levels")]
    [Range(0f, 10f)] public float sprintSpeedModifier = 1f;
    [Tooltip("How much will stamina levels affect walking noise produced?\n\nEx. If Stamina is at 50% and noiseModifier is at .5, noiseModifier will be modified by 50% of stamina levels")]
    [Range(0f, 10f)] public float noiseModifier = 1f;
    
    [Header("Stamina Drain")]
    public float walkStaminaDrain = 0f;
    public float sprintStaminaDrain = 4f;
    [SerializeField] float dashCost = 30f;
    [SerializeField] float stunCost = 30f;
    
    //[Header("Jumping")]
    //[SerializeField] [Range(1f, 500f)] float jumpForce = 250f;
    //[SerializeField] [Range(1f, 50f)] float airForward = 25f;
    //[SerializeField] float maxJumpsAllowed = Mathf.Infinity;
    //[SerializeField] [Tooltip("Diminishing upForce after each middair jump")]float diminishedUpForce = .85f;
    //public bool jump = false;
    //int jumpCounter = 0;
    //float currentUpForce;
    //float currentJumpsAllowed = 0;
    //float jumpTimer = 0f;
    //float jumpCooldown = .2f;
    
    // Cache
    Transform cameraPivot = null;
    Vector3 lastCollisionPoint = Vector3.zero;
    Vector3 direction = new Vector3();
    FeedingVictim currentVictim = null;
    [HideInInspector] public Feeder feeder = null;
    Health health = null;
    MaterialDimmer dimmer = null;

    private new void Awake()
    {
        base.Awake();
        dimmer = GetComponent<MaterialDimmer>();
        GetComponent<MovementTransform>().enabled = false;
        cameraPivot = transform.Find("CameraPivot");
        feeder = GetComponent<Feeder>();
        health = GetComponent<Health>();

        //currentUpForce = diminishedUpForce;
        //currentJumpsAllowed = maxJumpsAllowed;
        //jumpCounter = 0;
         
        //makeSoundEvent.AddListener(MakeSoundEffect);
    }

    //private void MakeSoundEffect(float intensity)
    //{
    //    Destroy(Instantiate(soundEffectFX, transform.position, Quaternion.identity, null), .2f);
    //}

    private new void Start()
    {
        base.Start();
        SetState(PlayerState.Idle);
    }

    public new void Update()
    {
        base.Update();

        // Player cannot control character while "latching on" to NPC or Dashing
        if (playerState == PlayerState.CommenceFeeding) { return; }
        if (playerState == PlayerState.Dashing) { return; }
        if (isStunned) { return; }

        ProcessMovementInput(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), out bool stopped);
        if(stopped)
        {
            animator.SetFloat("Speed", 0);
        }

        if (Input.GetKeyDown(dashKey))
        {
            if (stamina.GetStaminaValue() > dashCost)
            {
                //Dash forward
                stamina.ModifyStamina(-dashCost);
                StartCoroutine(Dash());
            }
            else
            {
                textSpawner.SpawnText("Too Tired!", Color.green);
            }
        }

        //if(Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal")!= 0f)
        //{
        //    walking = true;
        //}
        //else
        //{
        //    walking = false;
        //}

        //ProcessRaycast();
        if (Input.GetKeyDown(KeyCode.T))
        {
            if(objectInHand != null)
            {
                objectInHand.BePickedUp(this,false);
                DropObject(false);
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


    public override void MakeMovementSounds(AudioClip clip, float volumeScale)
    {
        if(Input.GetKey(sprintingKey))
        {
            PlaySoundEffect(sprintFootsteps, sprintFootStepsVolume);
        }
        else
        {
            base.MakeMovementSounds(clip, volumeScale);
        }
    }

    private IEnumerator Dash()
    {
        SetState(PlayerState.Dashing);
        //textSpawner.SpawnText("Dashing!", Color.green);
        
        //NavMeshHit hit;
        //NavMesh.Raycast(transform.position, DetermineDirectionOfMovement(1f, 0f), out hit, 1);
        //navMeshAgent.velocity = DetermineDirectionOfMovement(10f, 0f);// (new Vector3(0f, 0f, 2f);
        //navMeshAgent.SetDestination(hit.position);
        //this.hit = hit.position;
        //float moveSpeed = navMeshAgent.speed;
        //navMeshAgent.speed = 50f;
        //navMeshAgent.SetDestination(transform.position + transform.forward * 5);
        //while ((transform.position - navMeshAgent.destination).magnitude > navMeshAgent.stoppingDistance)
        //{
        //    yield return null;
        //}
        //navMeshAgent.speed = moveSpeed;

        navMeshAgent.Warp(transform.position + DetermineDirectionOfMovement(dashDistance, 0f));

        yield return new WaitForSeconds(1f);
        SetState(PlayerState.Idle);
        yield return null;
    }

    /***********************
    * ABILITY
    ***********************/
    public override void AnimationEventHit()
    {
        if (target.gameObject.tag == "Guard")
        {
            StunTarget(target.GetComponent<Character>(), guardStunVolume, guardStun);
        }
        else if (target.gameObject.tag == "Hunter")
        {
            StunTarget(target.GetComponent<Character>(), hunterStunVolume, hunterStun);
        }
        else
        {
            base.AnimationEventHit();
        }

    }
    //public override void StunTarget(Character target, float volume = 1, AudioClip clip = null)
    //{
    //    //animator.SetInteger("State", 3);
    //    //animator.ResetTrigger("CancelTriggerAttack");
    //    //animator.SetTrigger("TriggerAttack");
    //    base.StunTarget(target, volume, clip);
    //    //if(target.gameObject.tag == "Guard")
    //    //{
    //    //    base.StunTarget(target, guardStunVolume, guardStun);
    //    //}
    //    //if (target.gameObject.tag == "Hunter")
    //    //{
    //    //    base.StunTarget(target, hunterStunVolume, hunterStun);
    //    //}
    //    //else
    //    //{
            
    //    //}
    //}

    public override void AnimationEventResetAttack()
    {
        animator.SetInteger("State", 0);
        base.AnimationEventResetAttack();
    }

    public override void BecomeStunned()
    {
        animator.SetInteger("State", 5);
        base.BecomeStunned();
    }

    public override void BecomeUnStunned()
    {
        base.BecomeUnStunned();
        SetState(PlayerState.Idle);
    }

    public bool CheckStunConditions(Protector guard)
    {
        if (guard != null)
        {
            if (IsInRange(guard.transform.position, outgoingStunRange))
            {
                if (stamina.GetStaminaValue() > stunCost)
                {
                    stamina.ModifyStamina(-stunCost);
                    animator.SetInteger("State", 3);
                    //AnimationEventHit();
                    //if (guard.tag == "Hunter")
                    //{
                    //    StunTarget(guard, hunterStunVolume, hunterStun);
                    //}
                    //else if (guard.tag == "Guard")
                    //{
                    //    StunTarget(guard, guardStunVolume, guardStun);
                    //}

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
        SetState(PlayerState.CommenceFeeding);

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
            animator.SetInteger("State", 4);
            currentVictim.GetComponent<Character>().BecomeStunned(-1);
            feeder.AssignVictim(currentVictim);
            playerState = PlayerState.Feeding;
        }
        else
        {
            SetState(PlayerState.Idle);
        }
    }

    /***********************
    * STATE
    ***********************/

    public void Hide()
    {
        SetState(PlayerState.Hiding);
    }

    public void SetState(PlayerState newState)
    {
        print("PlayerState: " + newState);
        playerState = newState;
        switch(playerState)
        {
            case PlayerState.Idle:
                animator.SetInteger("State", 0);
                break;
            case PlayerState.CommenceFeeding:
                break;
            case PlayerState.Dashing:
                break;
            case PlayerState.Feeding:
                animator.SetInteger("State", 4);
                break;
            case PlayerState.Hiding:
                animator.SetInteger("State", 6);
                dimmer.DimMaterialColor();
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
    }

    /***********************
    * MOVEMENT
    ***********************/

    private void ProcessMovementInput(float verticalMag, float horizontalMag, out bool stopped)
    {
        direction = DetermineDirectionOfMovement(verticalMag, horizontalMag);
        stopped = true;
        if(allowKeyBoardTurn)
        {
            // Process Rotation
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Input.GetAxis("Rotation") * Vector3.up, turnSpeed * Time.deltaTime);
            }
        }

        // Process Jumping
        //if (jumpCounter < maxJumpsAllowed)
        //{
        //    if(jumpTimer <= 0f)
        //    {
        //        if (Input.GetButtonDown("Jump"))
        //        {
        //            Jump((jump) ? diminishedUpForce * jumpCounter : 1f);
        //            jumpTimer = jumpCooldown;
        //        }
        //        if (Input.GetButton("Jump"))
        //        {
        //            if (!jump) { Jump(1f); }
        //            jumpTimer = jumpCooldown;
        //        }
        //    }
        //    else
        //    {
        //        jumpTimer -= Time.deltaTime;
        //    }

        //}
        int animatorState = 0;
        // Process Translation
        if (horizontalMag != 0f || verticalMag != 0f)
        {
            animator.SetInteger("State", 1);
            stopped = false;
            animatorState = 1;
            if (currentInteractiable != null)
            {
                dimmer.RestoreMaterialColor();
                currentInteractiable.CancelInteract();
                textSpawner.SpawnText("Inturrupted", Color.yellow);
                SetState(PlayerState.Idle);
            }
            if (playerState == PlayerState.Feeding)
            {
                textSpawner.SpawnText("Inturrupted", Color.red);
                feeder.CancelFeeding();
                SetState(PlayerState.Idle);
            }
            //if (jump) // air controls
            //{
            //    //todo find an elegant way to control velocity while in air
            //    if (rigidBody.velocity.y > 0) // going up
            //    {
            //        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity + direction * airForward * Time.deltaTime, sprintSpeed);
            //    }
            //    else // going down
            //    {
            //        rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity + direction * Time.deltaTime, baseMovementSpeed);
            //    }
            //}
            //else // ground controls
            //{
            float staminaCost = walkStaminaDrain;
            // Determine how fast the player moves: running, walking speed

            float movementSpeed = baseMovementSpeed;
            float noiseLevel = walkingNoiseLevel;

            if (Input.GetKey(sprintingKey))
            {
                if (stamina.CheckCanAffordCost(sprintStaminaDrain))
                {
                    animatorState = 2;
                    staminaCost = sprintStaminaDrain;

                    //modify sprint speed by stamina percentage
                    // if stamina is at X%, movementSpeed is increased by X% of the sprintSpeed value * sprintModifier
                    movementSpeed = Mathf.Clamp(baseMovementSpeed + sprintSpeed * sprintSpeedModifier * stamina.GetStaminaPerc()
                        , baseMovementSpeed
                        , maxMovementSpeed);

                    //similarly for noise level
                    noiseLevel = Mathf.Clamp(walkingNoiseLevel + sprintingNoiseLevel * noiseModifier * (1f - stamina.GetStaminaPerc())
                        , walkingNoiseLevel
                        , maxNoiseLevel);
                }
            }
            else if (walkLouderIfLowStamina)
            {
                noiseLevel = Mathf.Clamp(walkingNoiseLevel + noiseModifier * (1f - stamina.GetStaminaPerc())
                        , 0f
                        , maxNoiseLevel);
            }
            actualMovementSpeed = movementSpeed;

            // The player model object turns toward the direction of movement
            model.forward = Vector3.Slerp(model.transform.forward, direction, turnSpeed * Time.deltaTime);

            // The object moves via navmesh in the direction of movement
            navMeshAgent.Move(direction * movementSpeed * Time.deltaTime);

            // Drain stamina accordingly
            stamina.ModifyStaminaShowOnIncrement(-staminaCost * Time.deltaTime);

            // Make noise accordingly
            makeSoundEvent.Invoke(noiseLevel);
            actualMovingNoiseLevel = noiseLevel;


        }
        else if (horizontalMag == 0f && verticalMag == 0f)
        {
            animatorState = 0;
            actualMovementSpeed = 0f;
            stopped = true;
        }
        //todo -- Update Animator with movement speed
        //actualMovementSpeed
        animator.SetFloat("Speed", actualMovementSpeed);
        //animator.SetInteger("State", animatorState);
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
    //private void Jump(float jumpScalar)
    //{
    //    jumpCounter++;

    //    if(navMeshAgent.enabled)
    //    {
    //        print("turning off nevmesh to jump");
    //        navMeshAgent.isStopped = true;
    //        navMeshAgent.enabled = false;
    //    }

    //    rigidBody.isKinematic = false;
    //    rigidBody.useGravity = true;
    //    jump = true;
    //    if (direction == Vector3.zero)
    //    {
    //        rigidBody.AddForce(Vector3.up * jumpForce * jumpScalar * Input.GetAxis("Jump"));
    //    }
    //    else
    //    {
    //        rigidBody.AddForce((Vector3.up + direction) * jumpForce * jumpScalar * Input.GetAxis("Jump"));
    //    }

    //}

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if ((collision.gameObject.tag == "Ground" || collision.gameObject.isStatic))
    //    {
    //        print("onground");
    //        lastCollisionPoint = transform.position;
    //    }

    //    if (jump)
    //    {
    //        if ((collision.gameObject.tag == "Ground" || collision.gameObject.isStatic))
    //        {
    //            jump = false;
    //            lastCollisionPoint = transform.position;
    //            GetComponent<MovementTransform>().enabled = false;
    //            print(collision.gameObject.name + " Static: " + collision.gameObject.isStatic);
    //            print("Collided with ground");

    //            rigidBody.isKinematic = true;
    //            rigidBody.useGravity = false;
    //            navMeshAgent.enabled = true;
    //            navMeshAgent.isStopped = false;
    //            jumpCounter = 0;
    //        }
    //        else
    //        {
    //            if (!collision.collider.isTrigger)
    //            {
    //                GetComponent<MovementTransform>().enabled = true;
    //                print("Landed on object that is not navmesh/static");
    //            }
    //        }
    //    }
    //}
}
