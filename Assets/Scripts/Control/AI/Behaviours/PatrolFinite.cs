using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolFinite : Patrol
{
    [Header("Patrol: Finite--")]
    Transform firstWaypoint = null;
    [SerializeField] int numLoops = 1;
    int loopCounter = 0;
    bool looping = true;

    private new void Awake()
    {
        base.Awake();
    }

    public new void OnEnable()
    {
        base.OnEnable();
        //print("finite enable");
        if(numLoops == 0)
        {
            print("numLoops =" + numLoops);
            looping = false;
            doneEvent.Invoke(this);
        }
        if(patrolRouteWaypoints2.Length > 0)
        {
            //print("setting first waypoint");
            firstWaypoint = patrolRouteWaypoints2[0].waypoint;
            loopCounter = 0;
        }
    }

    public new void Start()
    {
        //print("finite start");
        base.Start();
    }

    public new void Update()
    {
        base.Update();


        //if (ai == null) { return; }
        //print("finiteUpdate");

    }

    public override void ProcessWaiting()
    {
        if(looping)
        {
            base.ProcessWaiting();
        }
    }

    public override bool PrepareToEmbark()
    {
        //print("finite prepare embark");
        if (currentDestination == patrolRouteWaypoints2[patrolRouteWaypoints2.Length - 1].waypoint)
        {
            loopCounter++;
            
        }
        if(loopCounter == numLoops)
        {
            looping = false;
            //print("no more looping");
            doneEvent.Invoke(this);
        }
        else if(base.PrepareToEmbark())
        {
            //print("Finite embark");
            return true;
        }
        return false;
    }
}
