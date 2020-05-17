using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum NPCState { None, Default, Suspicious, Alert, Incapacitated, Dead }

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
//todo - give waitTimes before changing state random variation of +-2s
public class AIController : Character
{
    [Header("State")]
    [Header("NPC--")]
    public bool canSeePlayer = false;
    public NPCState currentState = NPCState.Default;
    public NPCState lastState = NPCState.Default;

    public AIBehaviour aIBehaviour = null;
    [HideInInspector]public string currentBehaviour = "";
    [HideInInspector]public string lastBehaviour = "";

    [SerializeField] float deEscalationWaitTime = 6f;
    private bool deEscalating;

    //[SerializeField] float waitTimeVariation = 2f;

    //[Header("Posting")]
    //[SerializeField] Vector3 postLocation = new Vector3();
    //[SerializeField] float totalPostTime = 20f;

    //[SerializeField] public string[] defaultBehaviourSequence = null;
    //[SerializeField] public string[] alertBehaviourSequence = null;
    //[SerializeField] public string[] suspiciousBehaviourSequence = null;

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

    public override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerHead = player.head;

        // Set State Presets
        foreach (BehaviourSequence b in stateSequences)
        {
            stateMap.Add(b.state, b.sequence);
        }
        //SetStatePresets();
    }

    public override void Start()
    {
        base.Start();
        SetBehaviourPresets();
    }
    private void SetBehaviourPresets()
    {
        foreach (BehaviourNode n in behaviourPresets)
        //for(int i = 0; i < behaviorNodes.Count; i++)
        {
            if (n.module != null)
            {
                n.module.enabled = false;
                behaviourMap.Add(n.behaviour, n.module);
            }
            else
            {
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
        if (isStunned) { return; }

        //todo -- achieve this without turning the head toward unnatural angles
        if (canSeePlayer)
        {
            head.LookAt(playerHead);
        }

        base.Update();

        if (currentState != lastState)
        {
            // If going to change state while waiting to de-escalate, cancel the de-escalation before changing the state
            if (deEscalating)
            {
                print(gameObject.name + " cancel de-escalation");
                deEscalating = false;
                CancelInvoke("DeEscalateState");
            }
            SetState();
        }

        if (currentBehaviour != lastBehaviour)
        {
            StartCurrentBehaviour();
        }
    }

    private AIBehaviour ParseBehaviourString(string s)
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

            case "Patrol":
                Patrol p = gameObject.AddComponent<Patrol>();
                p.ai = this;
                p.enabled = false;
                if(words[1].Contains(","))
                {
                    p.SetWaypoints(words[1].Split(','));
                }
                else
                {
                    p.SetWaypoints(FindObjectOfType<SceneWaypoints>().GetWaypoints(words[1]));
                }
                return p;
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
                    Debug.Log(e);
                    Type t = e.GetType();
                    if (t == typeof(ArgumentNullException) || t == typeof(FormatException) || t == typeof(IndexOutOfRangeException))
                    {
                        bw.duration = 0f;
                        bw.waitIndefinitly = true;
                    }
                }
                return bw;
            case "Wander":
                Wander w = gameObject.AddComponent<Wander>();
                w.ai = this;
                w.enabled = false;
                GameObject g;
                // If no specific region is given, find the first one
                if (words.Length < 2 || string.IsNullOrEmpty(words[1]))
                {
                    // todo: change to FindObjectsOfType and get the closest
                    WanderRegion potentialRegion = FindObjectOfType<WanderRegion>();
                    if (potentialRegion != null)
                    {
                        w.wanderRegion = potentialRegion;
                        if (w.wanderRegion != null)
                        {
                            return w;
                        }
                    }
                }
                // If a specific region is given, find a region with that name
                else
                {
                    g = GameObject.Find(words[1]);
                    if (g != null)
                    {
                        w.wanderRegion = g.GetComponent<WanderRegion>();
                        if (w.wanderRegion != null)
                        {
                            return w;
                        }
                    }
                }
                return null;
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
            //b.taskDone.RemoveAllListeners();

        //todo -- only destroy if it was added in runtime
            //if (!behaviourMap.ContainsValue(aIBehaviour))
            //{
            //    Destroy(aIBehaviour);
            //}
        }

        if (currentBehaviour == "Attack" && currentBehaviourSequence.Count == 0)
        {
            //Debug.Log("Need to go back");
            Attack a = (Attack)b;
            currentBehaviourSequence.Enqueue("GoToLocation:" + a.startLocation);
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
        else if(currentState != NPCState.Default)
        {
            deEscalating = true;
            Debug.Log(gameObject.name + " de-escalating in " + deEscalationWaitTime + " seconds.");
            Invoke("DeEscalateState", deEscalationWaitTime);
        }
    }









    /***********************
    * STATE
    ***********************/

    public override void Stun(float duration)
    {
        lastState = currentState;
        currentState = NPCState.Incapacitated;
        base.Stun(duration);
    }

    public override void UnStun()
    {
        currentState = lastState;
        lastState = NPCState.Incapacitated;
        base.UnStun();
    }

    private void SetState()
    {
        StopMoving();
        if (aIBehaviour != null)
        {
            aIBehaviour.doneEvent.Invoke(aIBehaviour);
        }

        if (stateMap.ContainsKey(currentState))
        {
            lastState = currentState;

            switch (currentState)
            {
                case NPCState.Alert:
                    indicator.Recolor(Color.red);
                    break;
                case NPCState.Suspicious:
                    //head.forward = model.forward;
                    indicator.Recolor(Color.yellow);
                    break;
                case NPCState.Default:
                    //head.forward = model.forward;
                    indicator.Recolor(Color.cyan);
                    break;
            }
            SetBehaviourSequence(stateMap[currentState]);
        }
    }

    private void DeEscalateState()
    {
        deEscalating = false;
        switch (currentState)
        {
            case NPCState.Alert:
                Debug.Log("Alert -> Suspicious");
                currentState = NPCState.Suspicious;
                break;
            case NPCState.Suspicious:
                Debug.Log("Suspicious -> Default");
                currentState = NPCState.Default;
                break;
            default:
                Debug.Log("Entering Default State");
                if (lastState == NPCState.Default)
                {
                    lastState = NPCState.None;
                }
                currentState = NPCState.Default;
                break;
        }
        deEscalating = false;
    }




    /***********************
    * BEHAVIOUR
    ***********************/

    private void StartCurrentBehaviour()
    {
        if (behaviourMap.ContainsKey(currentBehaviour) && behaviourMap[currentBehaviour] != null)
        {
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
        Debug.Log("Next Behaviour Sequence: " + t);

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
    public void PlayerSighted(bool sighted)
    {
        if (canSeePlayer && currentState != NPCState.Alert ||sighted && !canSeePlayer)
        {
            canSeePlayer = true;
            head.LookAt(playerHead.transform);
            
            textSpawner.SpawnText("A Vampire!!", Color.red);
            currentState = NPCState.Alert;

        }
        else if (!sighted && canSeePlayer)
        {
            canSeePlayer = false;
            //head.forward = model.forward;

            textSpawner.SpawnText("Where'd it go?", Color.yellow);

        }
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
