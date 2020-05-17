using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToObject : AIBehaviour
{
    [SerializeField] public GameObject objectReference = null;
    [SerializeField] public string objectName;
    public float range = 0f;

    public float distanceToReference;
    private bool arrivedAtObject = false;

    public new void Awake()
    {
        base.Awake();
    }

    // Start is called before the first frame update
    public new void Start()
    {
        base.Start();

        taskDone.AddListener(TaskDone);
        tasks.Add("GoToObject");
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
    }

    public new void Update()
    {
        base.Update();
        if (ai == null) { return; }

        if (objectReference != null)
        {
            distanceToReference = 
                Mathf.Min((transform.position - objectReference.transform.position).magnitude
                , (ai.head.position - objectReference.transform.position).magnitude);

            if (distanceToReference < range && !arrivedAtObject)
            {
                //ai.StopMoving();
                //Debug.Log("ArrivedAtObject");
                arrivedAtObject = true;
                tasks.Remove("GoToObject");
                taskDone.Invoke("GoToObject");
            }
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

    public new void OnEnable()
    {
        base.OnEnable();
        arrivedAtObject = false;

        range = Mathf.Max(ai.navMeshAgent.stoppingDistance + ai.navMeshDistanceBuffer, range);

        if(objectReference == null && objectName != null && objectName.Length > 0)
        {
            objectReference = GameObject.Find(objectName);
        }
        if (objectReference != null)
        {
            ai.MoveToDestination(objectReference.transform.position, .75f);
        }

        //if (TryGetReference(out objectReference))
        //{
        //    if (objectReference != null)
        //    {
        //        ai.MoveToDestination(objectReference.transform.position, .75f);
        //    }
        //}
    }
}


public class PickUpObject : GoToObject
{
    private new void Awake()
    {
        base.Awake();
    }

    public new void Start()
    {
        base.Start();

        tasks.Add("PickUp");
    }

    private void PickUp()
    {
        if (objectReference == null) { return; }

        if (distanceToReference < range /*& object not owned by another unit already*/)
        {
            Debug.Log("Picking up object: " + objectReference.name);
            objectReference.transform.rotation = transform.rotation;
            objectReference.transform.parent = transform;
        }
    }

    public override void TaskDone(string task = null)
    {
        if (task == "GoToObject")
        {
            PickUp();
            tasks.Remove("PickUpObject");
        }

        base.TaskDone();
    }

    public new void Update()
    {
        base.Update();
        if (ai == null) { return; }
    }

    public new void OnEnable()
    {
        base.OnEnable();
    }
}