using UnityEngine;

public class Villager : AIController 
{
    [Header("Villager--")]
    public bool sleepingBehavioursSet = false;

    public float sleepTime = 5f;
    public float wakeTime = 20f;
    [SerializeField] public Bed bed;
    [Header("Behaviours")]
    public string[] baseDefault = null;
    float zzzCounter = 2f;
    bool inBed = false;
    bool goingToSleep = false;
    //Cache
    FeedingVictim victim = null;
    
    public override void Awake()
    {
        base.Awake();
        victim = GetComponent<FeedingVictim>();
    }

    public override void Start()
    {
        base.Start();
    }

    public new void Update()
    {
        base.Update();

        if (bed == null) { return; }
        if (currentState != NPCState.Default) { return; }

        if (Time.time >= sleepTime && Time.time < wakeTime && !goingToSleep)
        {
            goingToSleep = true;
            MoveToDestination(bed.transform.position, 1f);
        }

        if (IsInRange(bed.transform.position, navMeshAgent.stoppingDistance + navMeshDistanceBuffer))
        {
            //print("At my bed");
            bed.Interact(this);
            inBed = true;
        }
        else
        {
            //print("Not in range yet");
            MoveToDestination(bed.transform.position, 1f);
        }

        if (inBed)
        {
            if (zzzCounter > 0f)
            {
                zzzCounter -= Time.deltaTime;
            }
            else
            {
                textSpawner.SpawnText("Zzz..", Color.yellow);
                zzzCounter = 4f;
            }
        }

        if(Time.time >= wakeTime && inBed)
        {
            inBed = false;
            bed.CancelInteract();
        }

        if (Time.time >= sleepTime && Time.time < wakeTime && !sleepingBehavioursSet)
        {
            sleepingBehavioursSet = true;

            if (stateMap.ContainsKey(NPCState.Default))
            {
                baseDefault = stateMap[NPCState.Default];
                string[] s = { "GoToObject:Torch", "Wait" };
                UpdateStateList(NPCState.Default, s);
                stateMap[NPCState.Default] = s;
            }
            //next task
        }


        if (Time.time >= wakeTime && sleepingBehavioursSet)
        {
            sleepingBehavioursSet = false;

            if (stateMap.ContainsKey(NPCState.Default))
            {
                UpdateStateList(NPCState.Default, baseDefault);
                stateMap[NPCState.Default] = baseDefault;
            }
        }








#if UNITY_EDITOR
        availableBehaviours = "GoToLocation, GoToObject, Patrol, Siren, Sleep, Wait, Wander";
#endif
    }

    void UpdateStateList(NPCState state, string[] sequence)
    {
        foreach(BehaviourSequence s in stateSequences)
        {
            if(s.state == state)
            {
                s.sequence = sequence;
                break;
            }
        }
    }

    //public void CommenceBeingFedOn(bool currentlyBeingFedOn)
    //{
    //    if (currentlyBeingFedOn)
    //    {
    //        Stun(-1);
    //    }
    //    else
    //    {
    //        currentState = NPCState.Alert;
    //    }
    //}
}
