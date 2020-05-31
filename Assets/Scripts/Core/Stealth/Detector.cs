using UnityEngine;

public class Detector : LineOfSight
{
    [Header("Detection")]
    public float detectedPercentage = 0f;
    public float suspiciousDetectionValue = .3f;
    float lastDetectedPercentage = 0f;
    NPCState lastState = NPCState.None;

    [Header("Sight")]
    public bool canSeePlayer = false;
    [SerializeField] [Range(0.1f,100f)]public float sightRange = 40f;
    [SerializeField] bool canSeeWhileSleeping = true;
    public float detectionEscalateRate = 4f;

    [Header("Hearing")]
    [SerializeField] [Range(0, 100f)] public float hearingThreshold = 20f;
    [SerializeField] bool canHearWhileSleeping = true;

    [Header("Debug")]
    [SerializeField] bool debugShowGizmos = false;

    AIController ai = null;
    DetectionTracker playerDetection = null;

    public new void Awake()
    {
        base.Awake();
        ai = GetComponent<AIController>();

        
        //playerDetectedEvent.AddListener(ai.PlayerSighted);
        //playerHeardEvent.AddListener(ai.PlayerHeard);
        fromTransform = ai.head;
    }

    private void Start()
    {
        playerDetection = ai.player.GetComponent<DetectionTracker>();
        if (ai.player != null)
        {
            player.makeSoundEvent.AddListener(CheckSoundDetection);
        }
    }

    public void CheckSoundDetection(float volume)
    {
        if (!canHearWhileSleeping && ai.currentBehaviour == "Sleep") { return; }
        //Debug.Log("Checking sound level");
        
        float distance = Vector3.Distance(player.transform.position, fromTransform.position);
        float resultingVolumeByDistance = volume + (volume * hearingThreshold)/(distance);

        //ai.textSpawner.SpawnText(volume.ToString() + " " + resultingVolumeByDistance, (resultingVolumeByDistance > hearingThreshold) ? Color.red : Color.yellow);
        if(resultingVolumeByDistance > hearingThreshold )
        {
            ai.PlayerHeard(true);
            detectedPercentage = .5f;
        }
    }

    private void Update()
    {
        if (player == null) { return; }
        if (player.isDead) { return; }
        //if (player.isHidden || player.playerState == PlayerState.Hiding) { canSeePlayer = false; ai.PlayerSighted(false); return; }
        if (!canSeeWhileSleeping && ai.currentBehaviour == "Sleep") { return; }
        
        if (ai.IsInRange(player.transform.position, sightRange) && !player.isHidden)
        {
            // if player is in front of ai
            if (Vector3.Dot((ai.playerHead.transform.position - ai.head.transform.position).normalized, ai.transform.forward) > 0)
            {
                // If player is in line of sight

                // Detect player immediately
                //if (RaycastToPlayerSuccessful(sightRange) &&  (!ai.canSeePlayer)) // todo: && if math result > vision threshold:
                //{
                //    print(gameObject.name + " sees player");
                //    ai.PlayerSighted(true);
                //}

                // Detect Player Over Time. Detection will occur faster as player gets closer
                if (RaycastToPlayerSuccessful(sightRange) && (!ai.canSeePlayer))
                {
                    if (DetectOnDelay() && ai.currentState != NPCState.Alert)
                    {
                        print("See player");
                        ai.PlayerSighted(true);
                    }
                    //else
                    //{
                    //    print(gameObject.name + " is beginning to see Vampire");
                    //}
                }

                if (ai.canSeePlayer)
                {
                    if (!RaycastToPlayerSuccessful(sightRange, out GameObject o))
                    {
                        Debug.Log(o.name);
                        ai.PlayerSighted(false);
                    }
                }
            }
            else
            {
                ai.PlayerSighted(false);
            }
        }
        //else
        //{
        //    RollOffDetection();
        //}

        if (ai.currentState == NPCState.Alert)
        {
            detectedPercentage = 1f;
        }
        else if(ai.currentState == NPCState.Suspicious)
        {
            if(detectedPercentage > suspiciousDetectionValue)
            {
                RollOffDetection();
            }
        }
        else
        {
            RollOffDetection();
        }

        if (gameObject.tag == "Hunter" || gameObject.tag == "Guard") 
        {
            if (detectedPercentage > 0f && (lastDetectedPercentage != detectedPercentage || lastState != ai.currentState))
            {
                lastState = ai.currentState;
                lastDetectedPercentage = detectedPercentage;
                playerDetection.AddToDetectedValue(this, ai.currentState);
            }
            else if (detectedPercentage == 0f && playerDetection.detectionDict.ContainsKey(this))
            {
                playerDetection.RemoveDetectionValue(this);
            }
        }
    }

    private void RollOffDetection()
    {
        if (detectedPercentage > 0f)
        {
            detectedPercentage = Mathf.Max(detectedPercentage - Time.deltaTime / detectionEscalateRate, (ai.currentState == NPCState.Suspicious) ? suspiciousDetectionValue : 0f);
        }
    }

    private bool DetectOnDelay()
    {
        float playerDistancePercentage = 1f - (player.transform.position - fromTransform.position).magnitude / sightRange;
        //print(playerDistancePercentage);
        detectedPercentage += detectionEscalateRate * Time.deltaTime * playerDistancePercentage;
        if (detectedPercentage >= 1f)
        {
            ai.PlayerSighted(true);
            return true;
        }
        return false;
    }

    public void OnDrawGizmos()
    {
        if(!debugShowGizmos) { return; }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(fromTransform.position, sightRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(fromTransform.position, hearingThreshold);
    }
}



// old code
//else if(ai.IsInRange(player.transform.position, suspiciousSightRange) && !ai.canSeePlayer)
//{
//    // detection will occur faster as player gets closer
//    detectedPercentage = 1f - (player.transform.position - fromTransform.position).magnitude / suspiciousSightRange;
//    if (RaycastToPlayerSuccessful(suspiciousSightRange) && (!ai.canSeePlayer))
//    {
//        if(DetectOnDelay())
//        {
//            detectedPercentage = 0f;
//            ai.PlayerSighted(true);
//        }
//    }
//    else
//    {
//        ai.PlayerSighted(false);
//    }
//}

//if (ai.canSeePlayer)
//{
//    if (!RaycastToPlayerSuccessful(out GameObject o))
//    {
//        Debug.Log(o.name + " 2");
//        //playerDetectedEvent.Invoke(false);
//        ai.PlayerSighted(false);
//    }
//}