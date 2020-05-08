using UnityEngine;
using UnityEngine.AI;

public class MovementNavMesh : MonoBehaviour
{
    [SerializeField] float maxMovementSpeed = 5f;
    [HideInInspector]public NavMeshAgent navMeshAgent = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    //public void StartMoveAction(Vector3 destination, float speedFraction)
    //{
    //    GetComponent<ActionScheduler>().StartAction(this);
    //    MoveTo(destination, speedFraction);
    //}

    public void MoveTo(Vector3 destination, float speedFraction)
    {
        //print("NavMesh destination: " + destination);
        navMeshAgent.destination = destination;
        navMeshAgent.speed = maxMovementSpeed * Mathf.Clamp01(speedFraction);
        navMeshAgent.isStopped = false;
    }

    public void MoveInDirection(Vector3 direction, float speed)
    {
        navMeshAgent.Move(direction * speed * Time.deltaTime);
    }

    public void StopMoving()
    {
        navMeshAgent.isStopped = true;
    }
}
