using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Flee : AIBehaviour
{
    [Header("Fleeing Behaviour")]
    [SerializeField] float fleeDistance = 40f;
    float fleeTime = 0f;
    float maxFleeDuration = 40f;
    Vector3 fleeDestination = new Vector3();
    PlayerController player = null;

    new void Awake()
    {
        base.Awake();
        
    }

    new void OnEnable()
    {
        base.OnEnable();
        player = ai.player;
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        if ((player.transform.position - transform.position).magnitude > fleeDistance)
        {
            doneEvent.Invoke(this);
        }
        else
        {
            if ((fleeDestination - transform.position).magnitude < ai.navMeshAgent.stoppingDistance)
            {
                ai.MoveToDestination(FindNewRandomFleeDestination(), 1.2f);
            }
        }
        base.Update();
    }

    private Vector3 FindNewRandomFleeDestination()
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        NavMeshHit hit;
        NavMesh.SamplePosition(direction, out hit, fleeDistance, 1);
        fleeDestination = hit.position;
        return fleeDestination;
    }
}
