﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToObject : AIBehaviour
{
    [Header("Go To Object--")]
    [SerializeField] public GameObject objectReference = null;
    [SerializeField] public string objectName;
    public float range = 0f;
    [SerializeField] [Range(0f, 10f)] float movementFraction = .75f;
    public float distanceToReference;
    private bool arrivedAtObject = false;

    public new void Awake()
    {
        base.Awake();
    }

    public new void OnEnable()
    {
        base.OnEnable();
        arrivedAtObject = false;

        range = Mathf.Max(ai.GetStoppingDistance(), range);

        if (objectReference == null && objectName != null && objectName.Length > 0)
        {
            objectReference = GameObject.Find(objectName);
        }
        if (objectReference != null)
        {
            ai.MoveToDestination(objectReference.transform.position, movementFraction);
        }

        //if (TryGetReference(out objectReference))
        //{
        //    if (objectReference != null)
        //    {
        //        ai.MoveToDestination(objectReference.transform.position, .75f);
        //    }
        //}
    }

    public new void Start()
    {
        base.Start();

        taskDone.AddListener(TaskDone);
        tasks.Add("GoToObject");
    }
    
    public new void Update()
    {
        base.Update();
        if (ai == null) { return; }

        if (objectReference != null)
        {
            //distanceToReference = 
            //    Mathf.Min((transform.position - objectReference.transform.position).magnitude
            //    , (ai.head.position - objectReference.transform.position).magnitude);

            if (ai.IsInRange(objectReference.transform.position)/*distanceToReference < range*/ && !arrivedAtObject)
            {
                //ai.StopMoving();
                //Debug.Log("ArrivedAtObject");
                arrivedAtObject = true;
                tasks.Remove("GoToObject");
                taskDone.Invoke("GoToObject");
            }
        }

    }

    public override void TaskDone(string task = null)
    {
        if (tasks.Count == 0)
        {
            taskDone.RemoveAllListeners();
            doneEvent.Invoke(this);

            //BehaviourEvent e = null;
            //if (ContainsEvent(typeof(BehaviourDoneEvent), out e))
            //{
            //    e.Invoke("Done");
            //}
        }
        else
        {
            print(tasks[0]);
        }
    }

    public bool TryGetReference(out GameObject reference)
    {
        reference = null;
        if (objectReference == null && objectName != null && objectName.Length > 0)
        {
            Type t = Type.GetType(objectName);
            UnityEngine.Object[] os = FindObjectsOfType(t);
            GameObject[] gos = new GameObject[os.Length];
            for (int i = 0; i < os.Length; i++)
            {
                gos[i] = (GameObject)os[i];
            }

            //todo: find the CLOSEST object with the name
            reference = gos[0];

            if (reference != null)
            {
                return true;
            }
        }

        return false;
    }


}


public class PickUpObject : GoToObject
{
    //[Header("Pick Up Object--")]
    private new void Awake()
    {
        base.Awake();
    }

    public new void OnEnable()
    {
        base.OnEnable();
    }

    public new void Start()
    {
        base.Start();
        tasks.Add("PickUpObject");
    }
    
    public new void Update()
    {
        base.Update();
        if (ai == null) { return; }
    }

    public override void TaskDone(string task = null)
    {
        PickUp pickup = objectReference.GetComponent<PickUp>();
        if (task == "GoToObject" && pickup != null)
        {
            pickup.BePickedUp(ai, true);
            tasks.Remove("PickUpObject");
        }
        base.TaskDone();
    }
}

public class DropObject : AIBehaviour
{
    private new void Awake()
    {
        base.Awake();
    }

    public new void OnEnable()
    {
        base.OnEnable();
        ai.DropObject(false);
        doneEvent.Invoke(this);
    }

    //public new void Start()
    //{
    //    base.Start();
    //}

    //public new void Update()
    //{
    //    base.Update();
    //    if (ai == null) { return; }
    //}
}