using System;
using UnityEngine;
using UnityEngine.AI;
public enum PlayerState { Idle, Feeding }

public class PlayerController : MonoBehaviour
{
    public PlayerState playerState = PlayerState.Idle;

    [Header("Movement")]
    [SerializeField] [Range(0f, 10f)] float walkingSpeed = 3f;
    [SerializeField] [Tooltip("While Holding LShift")] [Range(0f, 10f)] float runningSpeed = 7f;
    [SerializeField] [Range(45f, 270f)] float turnSpeed = 5f;
    Vector3 direction;

    [Header("Jump")]
    [SerializeField] [Range(1f, 500f)] float jumpForce = 5f;
    [SerializeField] [Range(1f, 50f)] float airForward = 5f;
    bool jump = false;
    int jumpCounter = 0;
    public int maxJumps = 2;

    [Header("Camera References")]
    [SerializeField] Transform cameraPivot = null;
    [SerializeField] Transform objectToSteer = null;

    // allow the player head to turn independently of the body in certain circumstances
    //[SerializeField] Transform objectToPan = null;
    //float panMinX = 5;
    //float panMaxX = -5;
    //float panMinY = -50;
    //float panMaxY = 50;

    // Cache
    NavMeshAgent navMeshAgent = null;
    Rigidbody rigidBody = null;
    Animator animator = null;
    //Feeder feeder = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        //feeder = GetComponent<Feeder>();
        jumpCounter = 0;
        ResetState();
    }

    private void ResetState()
    {
        playerState = PlayerState.Idle;
    }

    void Update()
    {
        //The player cannot move or trigger feeding while feeding
        if (playerState == PlayerState.Feeding)
        {
            return;
        }
        ProcessMovementInput(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
    }
    
    private void ProcessMovementInput(float verticalMag, float horizontalMag)
    {
        // Process Rotation
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Input.GetAxis("Rotation") * Vector3.up, turnSpeed * Time.deltaTime);
        }

        // Process Translation
        if (horizontalMag != 0f || verticalMag != 0f)
        {
            direction = DetermineDirectionOfMovement(verticalMag, horizontalMag);

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
                if (Input.GetKey(KeyCode.LeftShift))
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

            // Process Jumping
            if (jumpCounter < maxJumps)
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
        rigidBody.AddForce((Vector3.up + direction) * jumpForce * jumpScalar * Input.GetAxis("Jump"));
        jump = true;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (jump && collision.gameObject.tag == "Ground")
        {
            print("Collided with ground");
            jump = false;
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
            jumpCounter = 0;
        }
    }
}
