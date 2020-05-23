using UnityEngine;

public class Detector : LineOfSight
{
    [Header("Sight")]
    public bool canSeePlayer = false;
    [SerializeField] [Range(0,100f)]public float sightRange = 20f;
    [SerializeField] [Range(0,100f)]public float hearingThreshold = 20f;
    [SerializeField] bool canHearWhileSleeping = true;
    [SerializeField] bool canSeeWhileSleeping = true;
    [SerializeField] bool debugShowGizmos = false;
    [HideInInspector] public BoolEvent playerDetectedEvent;
    //[HideInInspector] public BoolEvent playerHeardEvent = new BoolEvent();

    //todo -- Hearing
    AIController ai = null;

    public new void Awake()
    {
        distance = sightRange;
        base.Awake();
        ai = GetComponent<AIController>();
        
        playerDetectedEvent.AddListener(ai.PlayerSighted);
        //playerHeardEvent.AddListener(ai.PlayerHeard);
        fromTransform = ai.head;
    }

    private void Start()
    {
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
            ai.textSpawner.SpawnText("!", Color.red);
            //playerHeardEvent.Invoke(true);
            ai.PlayerHeard(true);
        }
    }

    private void Update()
    {
        if (player.isDead) { return; }
        if (player.isHidden || player.playerState == PlayerState.Hiding) { canSeePlayer = false; playerDetectedEvent.Invoke(false); return; }
        if (!canSeeWhileSleeping && ai.currentBehaviour == "Sleep") { return; }

        if (ai.IsInRange(player.transform.position, sightRange))
        {
            // if player is in front of ai
            if (Vector3.Dot((ai.playerHead.transform.position - ai.head.transform.position).normalized, ai.transform.forward) > 0)
            {
                // If player is in line of sight
                if (RaycastToPlayerSuccessful() &&  (!ai.canSeePlayer)) // todo: && if math result > vision threshold:
                {
                    print(gameObject.name + " sees player");
                    
                    playerDetectedEvent.Invoke(true);
                }
                if(ai.canSeePlayer)
                {
                    if (!RaycastToPlayerSuccessful(out GameObject o))
                    {
                        //Debug.Log(o.name);
                        //playerDetectedEvent.Invoke(false);
                        ai.PlayerSighted(false);
                    }
                }

                // If the player is seen and goes out of line of sight
                //todo -- fix flip flopping
            }
            else
            {
                playerDetectedEvent.Invoke(false);
            }


        }

        //if (ai.canSeePlayer)
        //{
        //    if (!RaycastToPlayerSuccessful(out GameObject o))
        //    {
        //        Debug.Log(o.name + " 2");
        //        //playerDetectedEvent.Invoke(false);
        //        ai.PlayerSighted(false);
        //    }
        //}
    }

    public void OnDrawGizmos()
    {
        if(!debugShowGizmos) { return; }
        if (canSeePlayer)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
        Gizmos.DrawWireSphere(fromTransform.position, sightRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(fromTransform.position, hearingThreshold);
    }
}