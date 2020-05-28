using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Patrol : AIBehaviour
{

    [System.Serializable]
    public struct WaypointWaitBundle
    {
        public Transform waypoint;
        public float waitTime;
    }

    private Queue<WaypointWaitBundle> route = new Queue<WaypointWaitBundle>();

    [Header("Current State")] [Header("Patrol--")]
    public Transform currentDestination = null;
    public float currentWaitTime = 0f;
    public WaypointWaitBundle currentBundle;
    public bool arrived = false;
    float waitTimer = 0f;
    float secondsCounter = 1f;

    [Header("Patrol Route")]
    [SerializeField] Color gizmosColor = Color.white;
    [SerializeField] public WaypointWaitBundle[] patrolRouteWaypoints2 = null;


    [Header("Configure")]
    [SerializeField] [Range(0, 1f)] float patrolSpeedFraction = .5f;

    public new void Awake()
    {
        base.Awake();
    }

    public new void Start()
    {
        base.Start();

        if (patrolRouteWaypoints2.Length == 0)
        {
            print("No route to patrol");
            doneEvent.Invoke(this);
        }
        else
        {
            foreach (WaypointWaitBundle waypoint in patrolRouteWaypoints2)
            {
                //print("enqueuing " + waypoint);
                route.Enqueue(waypoint);
            }

            LeaveDestination();
        }
    }

    // Update is called once per frame
    public new void Update()
    {
        //print("baseUpdate");
        base.Update();
        //if (ai == null) { return; }
        if (currentDestination != null)
        {
            //print("current dest not null");
            if (ai.IsInRange(currentDestination.position, ai.navMeshAgent.stoppingDistance)/* && !arrived*/)
            {
                //print("Arrived");
                if (!arrived)
                {
                    arrived = true;
                    ArrivedAtDestination();
                }
                //ArrivedAtDestination();

                // Wait at Waypoint
                //if (waitTimer > 0f)
                //{
                //    ProcessWaiting();
                //}
                //else
                //{
                //    if (route2.Count > 0 && PrepareToEmbark())
                //    {
                //        LeaveDestination();
                //        //StartCoroutine(ArrivedStuff(currentBundle.waitTime));
                //    }
                //    else
                //    {
                //        currentDestination = null;
                //    }
                //}
            }
            //else if (!arrived && (transform.position - currentDestination.position).magnitude > ai.GetStoppingDistance())
            //{
            //    ai.MoveToDestination(currentDestination.position, patrolSpeedFraction);
            //}
        }
        else if (route.Count > 0)
        {
            //print("currentDestination = null");
            //print("current dest is null and there are other waypoints");
            LeaveDestination();
        }

        //if (currentDestination != null)
        //{
        //    if ((transform.position - currentDestination.position).magnitude < ai.navMeshAgent.stoppingDistance && !arrived)
        //    {
        //        ArrivedAtDestination();
        //    }
        //    else if (!arrived && (transform.position - currentDestination.position).magnitude > ai.navMeshAgent.stoppingDistance)
        //    {
        //        ai.MoveToDestination(currentDestination.position, patrolSpeedFraction);
        //    }
        //}
        //else if(route.Count > 0)
        //{
        //    currentDestination = route.Dequeue();
        //    ai.MoveToDestination(currentDestination.position, patrolSpeedFraction);
        //}
    }

    public virtual void ArrivedAtDestination()
    {
        // Wait at Waypoint?
        if (waitTimer > 0f)
        {
            StartCoroutine(ArrivedStuff(currentBundle.waitTime));
        }
        else
        {
            //Leave immediately
            if (route.Count > 0 && PrepareToEmbark())
            {
                LeaveDestination();
            }
            else
            {
                currentDestination = null;
            }
        }
    }

    private IEnumerator ArrivedStuff(float delay)
    {
        //print("arrived coroutine");
        //todo: anything that needs to happen as a function of TIME
        yield return new WaitForSeconds(1f);
        if (delay > 0f)
        {
            //face direction of waypoint forward
            while (transform.forward != currentBundle.waypoint.forward)
            {
                ProcessWaiting();
                yield return null;
            }
            yield return new WaitForSeconds(delay);
        }

        if (route.Count > 0 && PrepareToEmbark())
        {
            LeaveDestination();
        }
        else
        {
            currentDestination = null;
        }
    }

    public virtual bool PrepareToEmbark()
    {
        //print("base prepare embark");
        route.Enqueue(currentBundle);
        secondsCounter = 0f;
        return true;
    }

    public void ProcessTurning()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, currentDestination.rotation, 5f * Time.deltaTime);
    }

    public virtual void ProcessWaiting()
    {
        //print("Process Waiting");
        if (transform.forward != currentDestination.forward)
        {
            ProcessTurning();
        }
        else
        {
            if (secondsCounter > 0f)
            {
                secondsCounter -= Time.deltaTime;
            }
            else
            {
                ai.textSpawner.SpawnText(string.Format("Sentry: {0:0}", waitTimer), Color.white);
                secondsCounter = 1f;
            }

            waitTimer -= Time.deltaTime;
        }
    }
    
    public virtual void LeaveDestination()
    {
        currentBundle = route.Dequeue();
        currentDestination = currentBundle.waypoint;
        waitTimer = currentBundle.waitTime;
        ai.MoveToDestination(currentDestination.position, patrolSpeedFraction);
        arrived = false;
    }

    // Draw Route
    private void OnDrawGizmos()
    {
        if (patrolRouteWaypoints2.Length <= 0) { return; }

        for (int i = 0; i < patrolRouteWaypoints2.Length; i++)
        {
            int j = i + 1; //GetNextIndex(i);
            if (i + 1 == patrolRouteWaypoints2.Length)
            {
                j = 0;
            }
            if (patrolRouteWaypoints2[i].waypoint == null) { return; }

            Gizmos.color = gizmosColor;
            Gizmos.DrawSphere(GetWaypointPosition(i), .2f);
            Gizmos.DrawLine(GetWaypointPosition(i), GetWaypointPosition(j));
        }
    }

    public Vector3 GetWaypointPosition(int i)
    {
        if (i == patrolRouteWaypoints2.Length)
        {
            i = 0;
        }
        return patrolRouteWaypoints2[i].waypoint.position;
    }




    //public void SetWaypoints(string[] names)
    //{
    //    patrolRouteWaypoints = new Transform[names.Length];
    //    string st = "";
    //    foreach (string n in names)
    //    {
    //        st += n + ", ";
    //    }
    //    //Debug.Log(st);
    //    route.Clear();

    //    for (int i = 0; i < names.Length; i++)
    //    {
    //        GameObject g = GameObject.Find(names[i]);
    //        if (g != null)
    //        {
    //            //Debug.Log(names[i]);
    //            route.Enqueue(g.transform);
    //            patrolRouteWaypoints[i] = g.transform;
    //        }
    //    }
    //}

    //public virtual void ArrivedAtDestination()
    //{
    //    print("Arrived");
    //    arrived = true;
    //    if (route2.Count > 0)
    //    {
    //        print("enqueue");
    //        route2.Enqueue(currentBundle);
    //        // todo: whatever else needs to happen between arrival at and leaving a waypoint
    //        StartCoroutine(ArrivedStuff(currentBundle.waitTime));
    //    }
    //    //if (route.Count > 0)
    //    //{
    //    //    route.Enqueue(currentDestination);
    //    //    // todo: whatever else needs to happen between arrival at and leaving a waypoint
    //    //    StartCoroutine(ArrivedStuff(Random.Range(0f, maxPauseAtWaypoint)));
    //    //}
    //    else
    //    {
    //        currentDestination = null;
    //    }
    //}

    //private IEnumerator ArrivedStuff(float delay)
    //{
    //    print("arrived coroutine");
    //    //todo: anything that needs to happen as a function of TIME
    //    if(delay > 0f)
    //    {
    //        //face direction of waypoint forward
    //        while (transform.forward != currentBundle.waypoint.forward)
    //        {
    //            transform.forward = Vector3.Lerp(transform.forward, currentBundle.waypoint.forward, 20f * Time.deltaTime);
    //            yield return null;
    //        }
    //        yield return new WaitForSeconds(delay);
    //    }
    //    LeaveDestination();
    //}


    //private void LeaveDestination()
    //{
    //    currentDestination = route.Dequeue();

    //    ai.MoveToDestination(currentDestination.position, .75f);
    //    arrived = false;
    //}

    //public void AddWaypoint(Transform waypoint)
    //{
    //    route.Enqueue(waypoint);
    //}


}



