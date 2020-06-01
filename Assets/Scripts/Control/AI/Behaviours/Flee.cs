using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Flee : AIBehaviour
{
    [Header("Flee--")]
    [SerializeField] float fleeDistance = 40f;
    float fleeTime = 0f;
    float maxFleeDuration = 40f;
    Vector3 fleeDestination = new Vector3();
    PlayerController player = null;
    [SerializeField] [Range(0f, 10f)] float movementFraction = 2.2f;

    new void Awake()
    {
        base.Awake();
    }

    new void OnEnable()
    {
        base.OnEnable();
        player = ai.player;
        fleeTime = 0f;
        ai.MoveToDestination(FindNewRandomFleeDestination(), movementFraction);
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        fleeTime += Time.deltaTime;
        if ((player.transform.position - transform.position).magnitude > fleeDistance || ai.IsInRange(fleeDestination))
        {
            doneEvent.Invoke(this);
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
