using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : AIBehaviour
{
    private Queue<Transform> route = new Queue<Transform>();

    [Header("Current State")]
    public Transform currentDestination = null;
    public bool arrived = false;
    [SerializeField] Transform[] patrolRouteWaypoints = null;
    
    [Header("Configure")]
    [SerializeField] [Range(0, 10f)] float maxPauseAtWaypoint = 2f;
    [SerializeField] [Range(0, 1f)] float patrolSpeedFraction = .5f;

    new void Awake()
    {
        base.Awake();
    }

    private new void Start()
    {
        base.Start();
        foreach (Transform t in patrolRouteWaypoints)
        {
            route.Enqueue(t);
        }
    }

    // Update is called once per frame
    public new void Update()
    {
        base.Update();
        if (ai == null) { return; }
        if (currentDestination != null)
        {
            if ((transform.position - currentDestination.position).magnitude < ai.navMeshAgent.stoppingDistance && !arrived)
            {
                ArrivedAtDestination();
            }
            else if(!arrived && (transform.position - currentDestination.position).magnitude > ai.navMeshAgent.stoppingDistance)
            {
                ai.MoveToDestination(currentDestination.position, patrolSpeedFraction);
            }
        }
        else if(route.Count > 0)
        {
            currentDestination = route.Dequeue();
            ai.MoveToDestination(currentDestination.position, patrolSpeedFraction);
        }
    }

    private void OnDisable()
    {
        //ai.MoveToDestination(transform.position, .75f);
    }

    public void SetWaypoints(string[] names)
    {
        patrolRouteWaypoints = new Transform[names.Length];
        string st = "";
        foreach(string n in names)
        {
            st += n + ", ";
        }
        //Debug.Log(st);
        route.Clear();

        for(int i = 0; i < names.Length; i++)
        {
            GameObject g = GameObject.Find(names[i]);
            if(g != null)
            {
                //Debug.Log(names[i]);
                route.Enqueue(g.transform);
                patrolRouteWaypoints[i] = g.transform;
            }
        }
    }

    public void SetWaypoints(Transform[] waypoints)
    {
        patrolRouteWaypoints = new Transform[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            Transform waypoint = waypoints[i];
            if (waypoint != null)
            {
                route.Enqueue(waypoint);
                patrolRouteWaypoints[i] = waypoint;
            }
        }
    }

    private void ArrivedAtDestination()
    {
        arrived = true;
        if (route.Count > 0)
        {
            route.Enqueue(currentDestination);
            // todo: whatever else needs to happen between arrival at and leaving a waypoint
            StartCoroutine(ArrivedStuff(Random.Range(0f, maxPauseAtWaypoint)));
        }
        else
        {
            currentDestination = null;
        }
    }

    private IEnumerator ArrivedStuff(float delay)
    {
        //todo: anything that needs to happen as a function of TIME
        yield return new WaitForSeconds(delay);
        LeaveDestination();
    }

    private void LeaveDestination()
    {
        currentDestination = route.Dequeue();
        ai.MoveToDestination(currentDestination.position, .75f);
        arrived = false;
    }

    //public void AddWaypoint(Transform waypoint)
    //{
    //    route.Enqueue(waypoint);
    //}


}



