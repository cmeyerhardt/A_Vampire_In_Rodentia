using UnityEngine;

public class Detector : LineOfSight
{
    [Header("Sight")]
    public bool canSeePlayer = false;
    [SerializeField] float sightRange = 20f;
    [HideInInspector] public BoolEvent playerDetectedEvent;

    //todo -- Hearing
    AIController ai = null;

    public new void Awake()
    {
        distance = sightRange;
        base.Awake();
        ai = GetComponent<AIController>();
        playerDetectedEvent.AddListener(ai.PlayerSighted);
        fromTransform = ai.head;
    }

    private void Update()
    {
        if (player.isHidden || player.playerState == PlayerState.Hiding) { canSeePlayer = false; return; }

        // if player is in front of ai
        if (Vector3.Dot((ai.playerHead.transform.position - ai.head.transform.position).normalized, ai.transform.forward) > 0)
        {
            // If player is in line of sight and ai cant already see the player
            if (RaycastToPlayerSuccessful())
            {
                playerDetectedEvent.Invoke(true);
            }

            // If the player is seen and goes out of line of sight
            //todo -- fix flip flopping
        }
        else
        {
            playerDetectedEvent.Invoke(false);
        }
    }

    //private void OnDrawGizmos()
    //{
    //    if(canSeePlayer)
    //    {
    //        Gizmos.color = Color.red;
    //    }
    //    else
    //    {
    //        Gizmos.color = Color.yellow;
    //    }
    //    Gizmos.DrawSphere(transform.position + (4.5f * Vector3.up), .4f);
    //}
}