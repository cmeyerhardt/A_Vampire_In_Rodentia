using UnityEngine;
using UnityEngine.AI;
public enum PlayerState { Idle, Feeding }

public class PlayerController : MonoBehaviour
{
    public PlayerState playerState = PlayerState.Idle;

    [Header("Movement")]
    [SerializeField] [Range(0f, 10f)] float walkingSpeed = 3f;
    [SerializeField] [Tooltip("While Holding LShift")] [Range(0f, 10f)] float runningSpeed = 7f;
    [SerializeField] [Range(1f, 20f)] float turnSpeed = 5f;

    // Cache
    Vector3 direction = new Vector3();
    NavMeshAgent navMeshAgent = null;
    Feeder feeder = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        feeder = GetComponent<Feeder>();
        ResetState();
    }

    void Update()
    {
        //The player cannot move or trigger feeding while feeding
        if(playerState == PlayerState.Feeding)
        {
            return;
        }

        float movementSpeed = walkingSpeed;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = runningSpeed;
        }

        direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        navMeshAgent.Move(direction * movementSpeed * Time.deltaTime);
        transform.forward = Vector3.Slerp(transform.forward, direction, turnSpeed * Time.deltaTime);

        
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit[] hits = Physics.RaycastAll(ray);
        //    foreach (RaycastHit hit in hits)
        //    {
        //        if (hit.transform.GetComponent<FeedingVictim>() != null)
        //        {
        //            CommenceFeeding(hit);
        //            break;
        //        }
        //    }
        //}
    }

    //private void CommenceFeeding(RaycastHit hit)
    //{
    //    feedingState.SetActive(true);
    //    print("Feeding on " + hit.transform.name);
    //    playerState = PlayerState.Feeding;
    //    // animation state change
    //    feeder.Feed(hit.transform.GetComponent<FeedingVictim>());

    //    Invoke("ResetState", feedingDuration);
    //}

    private void ResetState()
    {
        playerState = PlayerState.Idle;

    }
}
