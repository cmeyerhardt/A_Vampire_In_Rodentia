using System;
using UnityEngine;
using UnityEngine.AI;

public enum PlayerState { Idle, Walking, Jumping, Feeding }

public class PlayerController : MonoBehaviour
{
    public PlayerState playerState = PlayerState.Idle;

    [Header("Movement")]
    [SerializeField] [Range(0f, 20f)] float walkingSpeed = 7f;
    [SerializeField] [Range(0f, 20f)] float runningSpeed = 10f;
    //[SerializeField] [Range(10f, 30f)] float dashSpeed = 20f;
    [SerializeField] [Range(45f, 270f)] float turnSpeed = 5f;
    [SerializeField] KeyCode sprintingKey = KeyCode.LeftShift;
    //[SerializeField] KeyCode dashKey = KeyCode.LeftControl;

    Vector3 direction;

    [Header("Jump")]
    [SerializeField] [Range(1f, 500f)] float jumpForce = 5f;
    [SerializeField] [Range(1f, 50f)] float airForward = 5f;
    bool jump = false;
    int jumpCounter = 0;
    public int maxJumpsAllowed = 2;

    [Header("Camera References")]
    [SerializeField] Transform cameraPivot = null;
    [SerializeField] Transform objectToSteer = null;
    Vector3 lastCollisionPoint = Vector3.zero;

    [Header("Debug")]
    [SerializeField] FloatingTextSpawner textSpawner = null;

    // Cache
    FeedingVictim currentVictim = null;
    NavMeshAgent navMeshAgent = null;
    Rigidbody rigidBody = null;
    Animator animator = null;
    Stamina stamina = null;
    Feeder feeder = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        stamina = GetComponent<Stamina>();
        feeder = GetComponent<Feeder>();
        jumpCounter = 0;
        ResetState();
    }

    private void Start()
    {
        feeder.feedEvent.AddListener(stamina.ModifyStamina);
        stamina.deathEvent.AddListener(Die);
    }

    private void ResetState()
    {
        playerState = PlayerState.Idle;
    }

    void Update()
    {
        ProcessMovementInput(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                FeedingVictim victim = hit.transform.GetComponent<FeedingVictim>();

                //The player cannot trigger feeding while feeding
                if (playerState != PlayerState.Feeding && victim != null)
                {
                    if(IsInRange(victim.transform, feeder.GetFeedingDistance()))
                    {
                        CommenceFeeding(victim);
                    }
                    else
                    {
                        print("Get Closer To Feed");
                    }
                    break;
                }
            }
        }
    }

    private void CommenceFeeding(FeedingVictim victim)
    {
        navMeshAgent.isStopped = true;
        // Change State
        playerState = PlayerState.Feeding;
        currentVictim = victim;

        victim.BeginFedOn();
        feeder.AssignVictim(currentVictim.transform.GetComponent<FeedingVictim>());
    }

    private bool IsInRange(Transform checkRange, float range)
    {
        return Vector3.Distance(transform.position, checkRange.position) < range;
    }

    private void ProcessMovementInput(float verticalMag, float horizontalMag)
    {
        direction = DetermineDirectionOfMovement(verticalMag, horizontalMag);

        // Process Rotation
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Input.GetAxis("Rotation") * Vector3.up, turnSpeed * Time.deltaTime);
        }

        // Process Translation
        if (horizontalMag != 0f || verticalMag != 0f)
        {
            if(playerState == PlayerState.Feeding)
            {
                playerState = PlayerState.Idle;
                feeder.CancelFeeding();
            }
            if (jump) // air controls
            {
                //todo should not have to break this up into conditions. find a way to limit velocity while in air
                if (rigidBody.velocity.y > 0) // going up
                {
                    rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity + direction * airForward * Time.deltaTime, runningSpeed);
                }
                else // going down
                {
                    rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity + direction * Time.deltaTime, walkingSpeed);
                }
            }
            else // ground controls
            { 
                // Determine how fast the player moves: running, walking speed
                float movementSpeed = walkingSpeed;
                if (Input.GetKey(sprintingKey))
                {
                    movementSpeed = runningSpeed;
                }
                
                // The player model object turns toward the direction of movement
                objectToSteer.forward = Vector3.Slerp(objectToSteer.transform.forward, direction, turnSpeed * Time.deltaTime);

                // The object moves via navmesh in the direction of movement
                navMeshAgent.Move(direction * movementSpeed * Time.deltaTime);

                //Update animator
                //animator.SetFloat("ForwardSpeed", movementSpeed);
            }
        }
        // Process Jumping
        if (jumpCounter < maxJumpsAllowed)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump((jump) ? .85f : 1f);
            }
            if (Input.GetButton("Jump"))
            {
                if (!jump) { Jump(1f); }
            }
        }
    }

    private Vector3 DetermineDirectionOfMovement(float verticalMag, float horizontalMag)
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

    //! Jumping
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
                    print("Landed on object that is not navmesh/static");
                }
            }
        }
    }

    private void Die()
    {
        textSpawner.SpawnText("I'm BLLUUUHHH.. dead", Color.red);
    }

}
