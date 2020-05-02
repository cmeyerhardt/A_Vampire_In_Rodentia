using UnityEngine;
using UnityEngine.AI;

public class MovementNavMesh : MonoBehaviour
{
    [SerializeField] [Range(0f, 50f)] float maxSpeed = 10f;
    [SerializeField] [Range(0f, 50f)] float walkingSpeed = 10f;
    [SerializeField] [Range(0f, 50f)] float sneakingSpeed = 10f;

    public NavMeshAgent navMeshAgent = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void MoveTowards(Vector3 direction)
    {
        navMeshAgent.speed = maxSpeed;
        navMeshAgent.Move(direction);
        navMeshAgent.isStopped = false;
    }

    public void StopMoving()
    {
        navMeshAgent.isStopped = true;
    }
}
