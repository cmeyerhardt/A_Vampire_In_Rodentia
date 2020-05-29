using UnityEngine;

public class Villager : AIController 
{
    [Header("Villager--")]
    public bool listeningToTime = false;

    [Header("Sleeping")]
    [SerializeField] public Bed bed;

    [Header("Behaviours")]
    public bool sleepingBehavioursSet = false;
    public string[] baseDefault = null;

    [Header("Mouse Color")]
    [SerializeField] SkinnedMeshRenderer skinnedMeshRend = null;
    [SerializeField] Color mouseFurColor = new Color();
    [SerializeField] Color mouseShirtColor = new Color();
    [SerializeField] bool randomizeMouseShirtColor = true;
    [SerializeField] bool randomizeRed = true;
    [SerializeField] bool randomizeGreen = true;
    [SerializeField] bool randomizeBlue = true;
    Material shirtMaterial = null;
    Material furMaterial = null;
    
    //Cache
    FeedingVictim victim = null;
    public Health health = null;

    public override void Awake()
    {
        base.Awake();
        NightCycle nightCycle = FindObjectOfType<NightCycle>();
        if(nightCycle != null)
        {
            nightCycle.timeEvent.AddListener(TimeChangeEvent);
            listeningToTime = true;
        }

        victim = GetComponent<FeedingVictim>();
        health = GetComponent<Health>();

        Material[] materials = skinnedMeshRend.materials;
        if(materials != null && materials.Length > 0)
        {
            furMaterial = materials[0];
            ChangeFurColor(0);
            shirtMaterial = materials[1];
            if (shirtMaterial != null)
            {
                if (randomizeMouseShirtColor || mouseShirtColor.a == 0f)
                {
                    Color randomShirtColor = new Color(randomizeRed ? Random.Range(0f, 1f) : 0f, randomizeGreen ? Random.Range(0f, 1f) : 0f, randomizeBlue ? Random.Range(0f, 1f) : 0f);
                    shirtMaterial.SetColor("_Color", randomShirtColor);
                }
                else if(!randomizeMouseShirtColor && mouseShirtColor.a > 0f)
                {
                    shirtMaterial.SetColor("_Color", mouseShirtColor);
                }
            }
        }
    }

    public void ChangeFurColor(float _ph)
    {
        if (furMaterial != null)
        {
            furMaterial.SetColor("_Color", mouseFurColor + (Color.white - mouseFurColor) * (1 - health.GetHealthPerc()));
        }
    }

    public override void Start()
    {
        base.Start();
        if(bed != null && behaviourMap.ContainsKey("Sleep"))
        { 
            if (((Sleep)behaviourMap["Sleep"]).bed == null)
            {
                ((Sleep)behaviourMap["Sleep"]).bed = bed;
            }
        }
    }

    public new void Update()
    {
        base.Update();
        
        if (currentState != NPCState.Default) { return; }

        //if (bed != null)
        //{
        //    if (Time.time >= sleepTime && Time.time < wakeTime && !sleepingBehavioursSet)
        //    {
        //        sleepingBehavioursSet = true;
        //        SetSleepBehaviour();
        //    }

        //    if (Time.time >= wakeTime && sleepingBehavioursSet)
        //    {
        //        sleepingBehavioursSet = false;
        //        SetDefaultBehaviours();
        //    }
        //}

#if UNITY_EDITOR
        availableBehaviours = "GoToLocation, GoToObject, PickUpObject, DropObject, Patrol, Siren, Sleep, Wait";
#endif
    }

    public void TimeChangeEvent(TimeSegment time)
    {
        //print("time is now " + time);
        switch(time)
        {
            case TimeSegment.Night:
                Invoke("SetSleepBehaviour", Random.Range(0,2f));
                break;
            case TimeSegment.Dawn:
                Invoke("SetDefaultBehaviours", Random.Range(0, 2f));
                    break;
            default:
                break;
        }
    }

    private void SetDefaultBehaviours()
    {
        if (bed == null) { return; }
        print("Setting default behaviours");
        if (stateMap.ContainsKey(NPCState.Default))
        {
            UpdateStateList(NPCState.Default, baseDefault);
            stateMap[NPCState.Default] = baseDefault;
            lastState = NPCState.None;
        }
    }

    private void SetSleepBehaviour()
    {
        if(bed == null) { return; }
        print("Setting sleep behaviours");
        if (stateMap.ContainsKey(NPCState.Default))
        {
            baseDefault = stateMap[NPCState.Default];
            string[] s = { "Sleep" };
            UpdateStateList(NPCState.Default, s);
            stateMap[NPCState.Default] = s;
        }
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
    public override void PlayerHeard(bool heard)
    {
        base.PlayerHeard(heard);
        switch (currentState)
        {
            case NPCState.Default:
                currentState = NPCState.Suspicious;
                break;
            case NPCState.Suspicious:
                break;
            default:
                break;
        }
    }

    public override AIBehaviour ParseBehaviourString(string s)
    {

        if(s.Split(':')[0] != null)
        {
            if (s.Split(':')[0] == "Sleep")
            {
                print("Parsing String for Sleep");
                Sleep sleep = gameObject.AddComponent<Sleep>();
                sleep.ai = this;
                sleep.enabled = false;
                sleep.bed = bed;
            }
        }
        return base.ParseBehaviourString(s);
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



//if (Time.time >= sleepTime && Time.time < wakeTime && !goingToSleep)
//{
//    goingToSleep = true;
//    MoveToDestination(bed.transform.position, 1f);
//}

//if (IsInRange(bed.transform.position, GetStoppingDistance()))
//{
//    //print("At my bed");
//    bed.Interact(this);
//    inBed = true;
//}
////else
////{
////    //print("Not in range yet");
////    MoveToDestination(bed.transform.position, 1f);
////}

//if (inBed)
//{
//    if (zzzCounter > 0f)
//    {
//        zzzCounter -= Time.deltaTime;
//    }
//    else
//    {
//        textSpawner.SpawnText("Zzz..", Color.yellow);
//        zzzCounter = 4f;
//    }
//}

////todo -- put in sleep on disable
//if(Time.time >= wakeTime && inBed)
//{
//    inBed = false;
//    bed.CancelInteract();
//}