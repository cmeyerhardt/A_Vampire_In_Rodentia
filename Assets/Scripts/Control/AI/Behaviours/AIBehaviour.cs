using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIBehaviour : MonoBehaviour
{
    public AIController ai = null;
    //public float exitTime = 0f;

    //public List<BehaviourEvent> behaviourEvents = new List<BehaviourEvent>();

    [HideInInspector] public BehaviourDoneEvent doneEvent = new BehaviourDoneEvent();
    [HideInInspector] public TaskDoneEvent taskDone = new TaskDoneEvent();

    // todo: change to queue of enums(?)
    [HideInInspector] public List<string> tasks = new List<string>();

    public void Awake()
    {
        //ai = GetComponent<AIController>();
        enabled = false;
        //AssignEvents();
    }

    public void Start()
    {
        

        //BehaviourEvent e = null;
        //if (ContainsEvent(typeof(TaskDoneEvent), out e))
        //{
        //    e.AddListener(TaskDone);
        //}
    }

    public void Update()
    {
        if(ai == null) { return; }
    }

    //public virtual void AssignEvents()
    //{
    //    behaviourEvents.Add(new BehaviourDoneEvent());
    //    behaviourEvents.Add(new TaskDoneEvent());
    //}

    public virtual void TaskDone(string task = null)
    {
        //if(tasks.Count == 0)
        //{
        //    doneEvent.Invoke(this);

        //    //BehaviourEvent e = null;
        //    //if (ContainsEvent(typeof(BehaviourDoneEvent), out e))
        //    //{
        //    //    e.Invoke("Done");
        //    //}
        //}
    }

    public virtual void OnEnable()
    {
        //if (ai != null)
        //{
        //    if (ai.indicator == null)
        //    {
        //        ai.indicator = ai.GetComponentInChildren<Colorizer>();
        //    }
        //}
    }

    //public bool ContainsEvent(Type type, out BehaviourEvent outEvent)
    //{
    //    outEvent = null;
    //    foreach (BehaviourEvent e in behaviourEvents)
    //    {
    //        if (e.GetType() == type)
    //        {
    //            outEvent = e;
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}

public class BehaviourDoneEvent : UnityEvent<AIBehaviour>
{ }

public class TaskDoneEvent : UnityEvent<string> { }

