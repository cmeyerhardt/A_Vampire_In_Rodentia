using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementNavMeshClickToMove : MonoBehaviour
{
    NavMeshAgent navMeshAgent = null;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                Vector3 location;
                if (HitNavMesh(hit, out location))
                {
                    navMeshAgent.destination = location;
                }
            }
        }
    }

    private bool HitNavMesh(RaycastHit hit, out Vector3 target)
    {
        NavMeshHit navMeshHit;
        target = new Vector3();
        if (!NavMesh.SamplePosition(hit.point, out navMeshHit, 100f, NavMesh.AllAreas)) { return false; }

        target = navMeshHit.position;

        return true;
    }
}
