using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : AIBehaviour
{
    [Header("State")]
    [SerializeField] public WanderRegion wanderRegion = null;
    public GameObject wanderTarget = null;
    public bool movingToRegion = false;

    [Header("Configure")]
    [SerializeField] public float delay = 2f;
    [SerializeField] public float maxDelayVariation = 2f;

    Coroutine wanderRoutine = null;
    public bool hasRoutine = false;

    new void Awake()
    {
        base.Awake();
    }

    private new void Start()
    {
        base.Start();
    }

    private new void OnEnable()
    {
        base.OnEnable();
        movingToRegion = false;
    }

    public new void Update()
    {
        hasRoutine = wanderRoutine == null ? false : true;
        base.Update();
        if (ai == null) { return; }
        if ((transform.position - wanderRegion.transform.position).magnitude > wanderRegion.radius && !movingToRegion)
        {
            ai.MoveToDestination(wanderRegion.transform.position, .75f);
            movingToRegion = true;
        }

        if ((transform.position - wanderRegion.transform.position).magnitude <= wanderRegion.radius)
        {
            movingToRegion = false;
            if (wanderTarget == null)
            {
                //Debug.Log("Setting Idle Target");
                wanderTarget = wanderRegion.GetRandomInteractible(gameObject);
            }

            if (wanderTarget != null && wanderRoutine == null)
            {
                wanderRoutine = StartCoroutine(InteractWithTarget());
            }
        }
    }

    private IEnumerator InteractWithTarget()
    {
        //Vector3 destination = wanderTarget.transform.position;
        while((wanderTarget.transform.position - transform.position).magnitude > ai.navMeshAgent.stoppingDistance)
        {
            //Debug.Log("Looping");
            if ((transform.position - wanderRegion.transform.position).magnitude > wanderRegion.radius)
            {
                wanderTarget = null;
                break;
            }

            ai.MoveToDestination(wanderTarget.transform.position, .75f);
            yield return null;
        }

        // I am no longer moving to the target (either I got to it, or it left the region)
        if(wanderTarget != null)
        {
            // I have reached my target
        }

        wanderRoutine = null;
        wanderTarget = null;
        //if(wanderTarget != null)
        //{
        //    //Debug.Log("Waiting");
        //    yield return new WaitForSeconds(delay + Random.Range(-maxDelayVariation,maxDelayVariation));
        //}
    }
}
