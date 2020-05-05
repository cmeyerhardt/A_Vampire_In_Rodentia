using System;
using UnityEngine;
using UnityEngine.AI;
public enum PlayerState { Idle, Feeding }

public class PlayerController : MonoBehaviour
{
    public PlayerState playerState = PlayerState.Idle;

    [Header("Movement Configure")]
    [SerializeField] [Range(0f, 10f)] float walkingSpeed = 3f;
    [SerializeField] [Tooltip("While Holding LShift")] [Range(0f, 10f)] float runningSpeed = 7f;
    [SerializeField] [Range(1f, 20f)] float turnSpeed = 5f;
    
    [Header("Camera References")]
    [SerializeField] Transform cameraPivot = null;
    [SerializeField] Transform objectToSteer = null;
    [SerializeField] Transform objectToPan = null;
    float panMinX = 5;
    float panMaxX = -5;
    float panMinY = -50;
    float panMaxY = 50;

    // Cache
    NavMeshAgent navMeshAgent = null;
    //Feeder feeder = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        //feeder = GetComponent<Feeder>();
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

        KeyboardInputMove(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        

    }

    private void KeyboardInputMove(float verticalMag, float horizontalMag)
    {
        if (horizontalMag != 0f || verticalMag != 0f)
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

            // Determine how fast the player moves: running, walking speed
            float movementSpeed = walkingSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movementSpeed = runningSpeed;
            }

            // Adjust the camera's normalized directional vectors by the magnitude of input
            Vector3 direction = _cameraForward * verticalMag + _cameraRight * horizontalMag;

            // The player model object turns toward the direction of movement
            objectToSteer.forward = Vector3.Slerp(objectToSteer.transform.forward, direction, turnSpeed * Time.deltaTime);

            // The object moves via navmesh in the direction of movement
            navMeshAgent.Move(direction * movementSpeed * Time.deltaTime);
        }
    }
}
