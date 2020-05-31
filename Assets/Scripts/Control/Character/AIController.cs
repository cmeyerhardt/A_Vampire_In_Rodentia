using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum NPCState { None, Default, Suspicious, Alert}

[System.Serializable]
public class BehaviourNode
{
    public string behaviour;
    //public NPCBehaviour behaviour;
    public AIBehaviour module;
}

[System.Serializable]
public class BehaviourSequence
{
    public NPCState state;
    public string[] sequence;
}

public class AIController : Character
{
    [Header("Audio")]
    [Header("NPC--")]
    [SerializeField] AudioClip[] suspiciousSounds = null;
    [SerializeField] [Range(0f, 1f)] public float suspiciousSoundMaxVolume = 1f;
    [SerializeField] public bool useSecondaryAudioSourceSuspiciousSound = false;


    [Header("State")]
    public bool canSeePlayer = false;
    public NPCState currentState = NPCState.Default;
    public NPCState lastState = NPCState.Default;
    public AIBehaviour aIBehaviour = null;
    public string currentBehaviour = "";
    [HideInInspector]public string lastBehaviour = "";

    [Header("State Behviours")]
    [SerializeField] [TextArea] public string availableBehaviours = "Attack, Patrol, Sleep";
    [SerializeField] public List<BehaviourSequence> stateSequences = new List<BehaviourSequence>();
    public Dictionary<NPCState, string[]> stateMap = new Dictionary<NPCState, string[]>();
    public Dictionary<string, AIBehaviour> behaviourMap = new Dictionary<string, AIBehaviour>();
    public Queue<string> currentBehaviourSequence = new Queue<string>();
    
    [Header("Behaviour Modules")]
    [SerializeField] public List<BehaviourNode> behaviourPresets = new List<BehaviourNode>();



    // Cache
    [HideInInspector] public PlayerController player = null;
    [HideInInspector] public Transform playerHead = null;
    Vector3 tetherPoint = new Vector3();
    [HideInInspector] public Detector detector = null;


    public override void Awake()
    {
        base.Awake();
        detector = GetComponent<Detector>();
        tetherPoint = transform.position;

        // todo: protect against null references
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerHead = player.head;
            
        }

        // Set State Presets
        foreach (BehaviourSequence b in stateSequences)
        {
            stateMap.Add(b.state, b.sequence);
        }
        //SetStatePresets();
        SetBehaviourPresets();
    }

    

    public override void Start()
    {
        base.Start();
        
    }
    private void SetBehaviourPresets()
    {
        foreach (BehaviourNode n in behaviourPresets)
        //for(int i = 0; i < behaviorNodes.Count; i++)
        {
            //print(gameObject + " adding behaviour: " + n.behaviour);
            if (n.module != null)
            {
                n.module.enabled = false;
                behaviourMap.Add(n.behaviour, n.module);
            }
            else
            {
                //print(gameObject + " creating new behaviour: " + n.behaviour);
                string tempBehaviour = n.behaviour.Split(':')[0];
                int c = 1;
                while (behaviourMap.ContainsKey(tempBehaviour + "" + c))
                {
                    c++;
                }
                tempBehaviour = tempBehaviour + "" + c;
                behaviourMap.Add(tempBehaviour, ParseBehaviourString(n.behaviour));
                n.behaviour = tempBehaviour;
                n.module = behaviourMap[n.behaviour];
            }
        }
    }

    public override void Update()
    {
        if (isDead) { return; }
        if (isStunned) { /*print(gameObject.name + " is stunned so i cant do anything");*/ return; }
        base.Update();

        if (currentState != lastState)
        {
            SetState();
        }

        if(currentState != NPCState.Alert && canSeePlayer)
        {
            currentState = NPCState.Alert;
        }

        if (currentBehaviour != lastBehaviour)
        {
            StartCurrentBehaviour();
        }


        // Loop state behaviours here
    }

    public virtual AIBehaviour ParseBehaviourString(string s)
    {
        string[] words = s.Split(':');
        //for(int i = 0; i < words.Length; i++)
        //{
        //    words[i] = words[i].Trim();
        //}
        switch (words[0])
        {
            case "GoTo":
            case "GoToObject":
                GoToObject bgto = gameObject.AddComponent<GoToObject>();
                bgto.ai = this;
                bgto.enabled = false;
                bgto.objectName = words[1];
                return bgto;
            case "GoToLocation":
                GoToLocation bgtl = gameObject.AddComponent<GoToLocation>();
                bgtl.ai = this;
                bgtl.enabled = false;
                if(words[1][0] == '(')
                {
                    bgtl.locationString = words[1];
                }
                else
                {
                    GameObject toGoTo = GameObject.Find(words[1]);
                    if(toGoTo != null)
                    {
                        bgtl.locationString = '(' + toGoTo.transform.position.x.ToString() + ',' + toGoTo.transform.position.y.ToString() + ',' + toGoTo.transform.position.z.ToString() + ')';
                    }
                }
                return bgtl;

            //case "Patrol":
            //    Patrol p = gameObject.AddComponent<Patrol>();
            //    //    p.ai = this;
            //    //    p.enabled = false;
            //    //    //if(words[1].Contains(","))
            //    //    //{
            //    //        p.SetWaypoints(words[1].Split(','));
            //    //    //}
            //    //    //else
            //    //    //{
            //    p.patrolRouteWaypoints = FindObjectOfType<SceneWaypoints>().GetWaypoints(words[1]);
            ////    //    p.SetWaypoints(FindObjectOfType<SceneWaypoints>().GetWaypoints(words[1]));
            ////    //}
            //    return p;
            case "PickUp":
            case "PickUpObject":
                PickUpObject u = gameObject.AddComponent<PickUpObject>();
                u.ai = this;
                u.enabled = false;
                u.objectName = words[1];
                return u;
            case "Wait":
                Wait bw = gameObject.AddComponent<Wait>();
                bw.ai = this;
                try
                {
                    bw.duration = float.Parse(words[1]);
                    bw.waitIndefinitly = false;
                }
                catch (Exception e)
                {
                    //Debug.Log(e);
                    Type t = e.GetType();
                    if (t == typeof(ArgumentNullException) || t == typeof(FormatException) || t == typeof(IndexOutOfRangeException))
                    {
                        bw.duration = 0f;
                        bw.waitIndefinitly = true;
                    }
                }
                return bw;
            //case "Wander":
            //    Wander w = gameObject.AddComponent<Wander>();
            //    w.ai = this;
            //    w.enabled = false;
            //    GameObject g;
            //    // If no specific region is given, find the first one
            //    if (words.Length < 2 || string.IsNullOrEmpty(words[1]))
            //    {
            //        // todo: change to FindObjectsOfType and get the closest
            //        WanderRegion potentialRegion = FindObjectOfType<WanderRegion>();
            //        if (potentialRegion != null)
            //        {
            //            w.wanderRegion = potentialRegion;
            //            if (w.wanderRegion != null)
            //            {
            //                return w;
            //            }
            //        }
            //    }
            //    // If a specific region is given, find a region with that name
            //    else
            //    {
            //        g = GameObject.Find(words[1]);
            //        if (g != null)
            //        {
            //            w.wanderRegion = g.GetComponent<WanderRegion>();
            //            if (w.wanderRegion != null)
            //            {
            //                return w;
            //            }
            //        }
            //    }
            //    return null;
            case "Idle":
            default:
                return null;
        }
    }



    private void BehaviourDone(AIBehaviour b = null)
    {
        if (aIBehaviour != null &&  b == aIBehaviour)
        {
            aIBehaviour.enabled = false;
            aIBehaviour.doneEvent.RemoveAllListeners();
            aIBehaviour = null;
        }

        if (b != null)
        {
            //Debug.Log(gameObject.name + " " + b.GetType() + " done.");
            b.enabled = false;
            b.doneEvent.RemoveAllListeners();
        }

        if (currentBehaviour == "Attack" && currentBehaviourSequence.Count == 0)
        {
            //Debug.Log("Need to go back");
            Attack a = (Attack)b;
            currentBehaviourSequence.Enqueue("GoToLocation:" + tetherPoint);
        }

        if(!behaviourMap.ContainsKey(currentBehaviour) && b != null)
        {
            Destroy(b);
        }

        if(currentBehaviourSequence.Count > 0)
        {
            currentBehaviour = currentBehaviourSequence.Dequeue();
        }
        // When no more behaviours for the current behaviour sequence, de-escalate state
        //else if(currentState != NPCState.Default)
        //{
        //    deEscalating = true;
        //    Debug.Log(gameObject.name + " de-escalating in " + deEscalationWaitTime + " seconds.");
        //    Invoke("DeEscalateState", deEscalationWaitTime);
        //}
        else
        {
            DeEscalateState();
        }
    }

    private bool DetermineIfRemoveComponent(AIBehaviour b)
    {
        foreach (BehaviourNode predefinedBehaviour in behaviourPresets)
        {
            if (predefinedBehaviour.module == b)
            {
                return false;
            }
        }
        return true;
    }

    public override void DropObject(bool objectTaken)
    {
        if(objectTaken)
        {
            currentState = NPCState.Suspicious;
        }
        base.DropObject(objectTaken);
    }


    /***********************
    * STATE
    ***********************/

    public override void BecomeStunned(float duration)
    {
        //lastState = currentState;
        //isStunned = true;
        base.BecomeStunned(duration);
    }

    public override void BecomeUnStunned()
    {
        if(isStunned)
        {
            if(canSeePlayer)
            {
                currentState = NPCState.Alert;
            }
            else
            {
                currentState = NPCState.Suspicious;
            }
            lastState = NPCState.None;
            base.BecomeUnStunned();
        }
    }

    private void SetState()
    {
        StopMoving();
        if (aIBehaviour != null)
        {
            //print("aIBehaviour now done" + aIBehaviour);
            //aIBehaviour.doneEvent.Invoke(aIBehaviour);
            aIBehaviour.enabled = false;
            aIBehaviour = null;
        }

        if (lastState == NPCState.Default || lastState == NPCState.None)
        {
            if (behaviourMap.ContainsKey("Return"))
            {
                //Debug.Log("trying to set return location");
                if (((GoToLocation)behaviourMap["Return"]).nullableLocation == null) //todo: keep this? prevents re-assigning "Return" location if it has already been assigned. effectively makes a permanent "Home" location
                {
                    //Debug.Log("Setting return location " + transform.position);
                    ((GoToLocation)behaviourMap["Return"]).nullableLocation = transform.position;
                }
            }
        }
        else if (currentState == NPCState.Suspicious)
        {
            if(suspiciousSounds.Length > 0)
            {
                PlaySoundEffect(suspiciousSounds[UnityEngine.Random.Range(0, suspiciousSounds.Length - 1)], suspiciousSoundMaxVolume, useSecondaryAudioSourceSuspiciousSound);
            }

        }

        if (behaviourMap.ContainsKey("GoToPlayer"))
        {
            //Debug.Log("Setting player location " + player.transform.position);
            ((GoToLocation)behaviourMap["GoToPlayer"]).nullableLocation = player.transform.position;
        }



        if (stateMap.ContainsKey(currentState))
        {
            lastState = currentState;
            SetBehaviourSequence(stateMap[currentState]);
        }
    }

    private void DeEscalateState()
    {
        switch (currentState)
        {
            case NPCState.Alert:
                currentState = NPCState.Suspicious;
                break;
            case NPCState.Suspicious:
                currentState = NPCState.Default;
                break;
            default:
                if (lastState == NPCState.Default)
                {
                    lastState = NPCState.None;
                }
                currentState = NPCState.Default;
                break;
        }
    }


    /***********************
    * BEHAVIOUR
    ***********************/

    private void StartCurrentBehaviour()
    {
        if (behaviourMap.ContainsKey(currentBehaviour) && behaviourMap[currentBehaviour] != null)
        {
            CancelInteract();
            behaviourMap[currentBehaviour].doneEvent.AddListener(BehaviourDone);
            behaviourMap[currentBehaviour].ai = this;
            behaviourMap[currentBehaviour].enabled = true;
            aIBehaviour = behaviourMap[currentBehaviour];
            lastBehaviour = currentBehaviour;
        }
        else
        {
            AIBehaviour a = ParseBehaviourString(currentBehaviour);
            if (a != null)
            {
                CancelInteract();

                a.doneEvent.AddListener(BehaviourDone);
                a.ai = this;
                a.enabled = true;
                aIBehaviour = a;
                lastBehaviour = currentBehaviour;
            }
            else
            {
                Debug.Log("Missing behaviour for " + gameObject.name + ": " + currentBehaviour);
                //currentBehaviour = lastBehaviour;
                BehaviourDone();
            }
        }
    }

    public void SetBehaviourSequence(string[] behaviourSequence)
    {
        currentBehaviour = "";
        lastBehaviour = "";
        currentBehaviourSequence.Clear();

        string t = "";
        foreach (string s in behaviourSequence)
        {
            string w = CleanUpBehaviourString(s);
            t += w + ", ";
            currentBehaviourSequence.Enqueue(w);
        }
        ////Debug.Log("Next Behaviour Sequence: " + t);

        currentBehaviour = currentBehaviourSequence.Dequeue();
    }

    private bool CreateBehaviour(string behaviourString, out AIBehaviour newBehaviour)
    {
        newBehaviour = ParseBehaviourString(behaviourString);
        return newBehaviour == null ? false : true;
    }


    /***********************
    * EVENT DRIVEN
    ***********************/
    public void PlayerSighted(bool alerted)
    {
        if (/*currentState != NPCState.Alert &&*/ alerted && !canSeePlayer)
        {
            canSeePlayer = true;
            textSpawner.SpawnText("A Vampire!", true, Color.red);
            currentState = NPCState.Alert;
            DropObject(false);
        }
        //else if (currentState == NPCState.Alert && alerted)
        //{
        //    timeSinceLastSawPlayer = 0f;
        //}
        else if (canSeePlayer && !alerted)
        {
            
            //lastSeenPlayerLocation = player.transform.position;
            if (behaviourMap.ContainsKey("GoToPlayer"))
            {
                //Debug.Log("Setting player location " + player.transform.position);
                ((GoToLocation)behaviourMap["GoToPlayer"]).nullableLocation = player.transform.position;
            }
            canSeePlayer = false;
            //DeEscalateState();
            //MoveToDestination(lastSeenPlayerLocation, 1f);
            //print("cannot see player");
            //textSpawner.SpawnText("Where'd it go?", Color.yellow);
        }
    }

    public virtual void PlayerHeard(bool heard)
    {
        //Debug.Log(gameObject + " heard the player.");
        
        //switch(currentState)
        //{
        //    case NPCState.Default:
        //        currentState = NPCState.Suspicious;
        //        break;
        //    case NPCState.Suspicious:
        //        lastState = NPCState.None;
        //        break;
        //    default:
        //        break;
        //}
    }

    /****************
    * PRIVATE METHODS
    *****************/
    private string CleanUpBehaviourString(string behaviourString)
    {
        string[] words = behaviourString.Split(':');
        string t = "";
        //foreach(string w in words)
        for (int i = 0; i < words.Length; i++)
        {
            t += words[i].Trim() + ((i + 1 < words.Length) ? ":" : "");
        }
        return t;
    }

    private List<RaycastHit> SortHitList(RaycastHit[] hits)
    {
        List<RaycastHit> outList = new List<RaycastHit>();

        foreach (RaycastHit hit in hits)
        {
            bool added = false;
            if (hit.transform.gameObject != null && hit.collider.gameObject != gameObject)
            {
                if (outList.Count == 0)
                {
                    outList.Add(hit);
                }
                else
                {
                    // insert into list IN CORRECT POSITION
                    for (int i = 0; i < outList.Count; i++)
                    {
                        if (outList[i].distance > hit.distance)
                        {
                            outList.Insert(i, hit);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        outList.Add(hit);
                    }
                }
            }
        }
        return outList;
    }

}
