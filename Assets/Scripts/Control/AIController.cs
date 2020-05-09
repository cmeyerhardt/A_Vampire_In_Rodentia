using System.Collections.Generic;
using UnityEngine;

public enum NPCState { Idle, Posting, Patrolling, Suspicious, Alert, Incapacitated, Sleeping, Siren, Death }
public enum PatrollingType { Sequential, Random }
public class AIController : MonoBehaviour
{
    [Header("State Machine")]
    public NPCState currentState = NPCState.Idle;
    [SerializeField] NPCState defaultBehaviour = NPCState.Idle;
    [SerializeField] NPCState afterBeingFedOnBehaviour = NPCState.Idle;
    [SerializeField] NPCState playerSeenBehaviour = NPCState.Idle;
    
    [SerializeField] float suspiciousWaitDuration = 5f;
    float suspicionWaitTime = 0f;
    
    [Header("Patrolling")]
    [SerializeField] PatrolPath patrolPath = null;
    [SerializeField] PatrollingType patrolType = PatrollingType.Sequential;
    [SerializeField] float patrolSpeedFraction = .5f;

    int currentPathIndex;
    Vector3 nextPosition;
    float pathTolerance = 2f;

    //[SerializeField] float patrolPauseDuration = 0f;
    //float patrolWaitTime = 0f;
    //float currentWaitTime = 0f;

    [Header("Sight")]
    [SerializeField] Sight sight = null;
    [SerializeField] float sightRange = 10f;

    //todo -- Hearing

    [Header("Debug")]
    public bool debugSeePlayer = false;
    [SerializeField] FloatingTextSpawner textSpawner = null;
    [SerializeField] Color postingColor = Color.magenta;
    [SerializeField] Color alertColor = Color.red;
    [SerializeField] Color suspiciousColor = Color.yellow;
    [SerializeField] Color idleColor = Color.green;
    [SerializeField] Color patrollingColor = Color.cyan;
    [SerializeField] Color incapacitateColor = Color.blue;
    [SerializeField] Color deathColor = Color.black;

    // Cache
    Vector3 postLocation = new Vector3();
    PlayerController player = null;
    MovementNavMesh movement = null;
    FeedingVictim victim = null;
    Colorizer indicator = null;
    Stamina stamina = null;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        movement = GetComponent<MovementNavMesh>();
        victim = GetComponent<FeedingVictim>();
        indicator = GetComponentInChildren<Colorizer>();
        stamina = GetComponent<Stamina>();
    }
    
    void Start()
    {
        victim.feedEvent.AddListener(stamina.ModifyStamina);
        victim.fedOnEvent.AddListener(CommenceBeingFedOn);
        stamina.deathEvent.AddListener(Die);
        postLocation = transform.position;
        ChangeState(defaultBehaviour, " I've just started");
        if(patrolPath != null)
        {
            GetNextWaypoint();
        }
        sight.SetRange(sightRange);
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCState.Incapacitated:
                return;

            case NPCState.Posting:
                PostingBehaviour();
                break;

            case NPCState.Patrolling:
                if (patrolPath != null)
                {
                    PatrollingBehaviour();
                }
                else
                {
                    ChangeState(NPCState.Posting, "I have no path to patrol so I'll just post");
                }
                break;
            case NPCState.Suspicious:
                SuspiciousBehaviour();
                break;

            case NPCState.Alert:
                AlertBehaviour();
                break;

            case NPCState.Idle:
            default:
                break;
        }

        if(debugSeePlayer)
        {
            ChangeState(NPCState.Alert, "I can see the player");
        }
    }

    private void AlertBehaviour()
    {
        if(!IsInRange(movement.navMeshAgent.stoppingDistance))
        {
            movement.MoveTo(player.transform.position, 1f);
        }
        else
        {
            print("Gotcha!");
        }
    }

    private void SuspiciousBehaviour()
    {
        suspicionWaitTime += Time.deltaTime;
        if (suspicionWaitTime > suspiciousWaitDuration)
        {
            ChangeState(defaultBehaviour, "I was suspicious for long enough");
            suspicionWaitTime = 0f;
        }
    }

    private void PatrollingBehaviour()
    {
        if ((transform.position - nextPosition).magnitude > pathTolerance)
        {
            //movement.MoveInDirection((nextPosition - transform.position).normalized, patrolSpeedFraction);
            movement.MoveTo(nextPosition, patrolSpeedFraction);
        }
        else
        {
            GetNextWaypoint();
        }

        //if (Random.Range(0, 7) % 2 == 0) // Will the guard wait at the next waypoint?
        //{
        //    currentWaitTime = Random.Range(0f, patrolPauseDuration);// How long will the guard wait?
        //    patrolWaitTime = 0f;
        //}
        //else
        //{
        //    currentWaitTime = 0f;
        //}

        //patrolWaitTime += Time.deltaTime;

        //if (patrolWaitTime >= currentWaitTime)
        //{
        //    nextPosition = GetNextWaypoint();
        //    print("Finding a new waypoint");
        //}

    }

    private void GetNextWaypoint()
    {
        if(patrolType == PatrollingType.Sequential)
        {
            currentPathIndex = patrolPath.GetNextIndex(currentPathIndex);
        }
        else if(patrolType == PatrollingType.Random)
        {
            currentPathIndex = patrolPath.GetRandomIndex();
        }
        else
        {
            print("There was an unexpected value associated with PatrolType: " + patrolType);
        }

        nextPosition = patrolPath.GetWaypoint(currentPathIndex);
    }

    private void PostingBehaviour()
    {
        if ((transform.position - postLocation).magnitude > 0f)
        {
            movement.MoveTo(postLocation, patrolSpeedFraction);
        }
    }
    
    public void CommenceBeingFedOn(bool currentlyBeingFedOn)
    {
        if(currentlyBeingFedOn)
        {
            //todo -- calculate change the NPC will resist the feed
            ChangeState(NPCState.Incapacitated, "I'm being fed on");
        }
        else
        {
            ChangeState(afterBeingFedOnBehaviour, "I was just fed on");
        }
    }
    
    public void PlayerSighted(bool sighted)
    {
        if(currentState != NPCState.Incapacitated)
        {
            if (sighted && currentState != playerSeenBehaviour)
            {
                ChangeState(playerSeenBehaviour, "The player has been sighted");
            }
            else if (!sighted && currentState != NPCState.Suspicious)
            {
                ChangeState(NPCState.Suspicious, "I lost sight of the player");
            }
        }
    }




    /*
     * PRIVATE METHODS
     */

    private void Die()
    {
        ChangeState(NPCState.Death, "I ran out of life force");
    }

    private void ChangeState(NPCState newState, string changeReason)
    {
        movement.StopMoving();
        currentState = newState;
        Color indicatorColor = Color.white;
        switch(currentState)
        {
            case NPCState.Incapacitated:
                indicatorColor = incapacitateColor;
                textSpawner.SpawnText("*Incapacitated*", incapacitateColor);
                break;
            case NPCState.Posting:
                indicatorColor = postingColor;
                textSpawner.SpawnText("*Posting*", postingColor);
                break;
            case NPCState.Suspicious:
                indicatorColor = suspiciousColor;
                textSpawner.SpawnText("*Suspicious*", suspiciousColor);
                break;
            case NPCState.Alert:
                indicatorColor = alertColor;
                textSpawner.SpawnText("*Alert*", alertColor);
                break;
            case NPCState.Patrolling:
                indicatorColor = patrollingColor;
                textSpawner.SpawnText("*Patrolling*", patrollingColor);
                break;
            case NPCState.Death:
                indicatorColor = deathColor;
                textSpawner.SpawnText("*Dying*", deathColor);
                break;
            case NPCState.Idle:
            default:
                textSpawner.SpawnText("*Idle*");
                indicatorColor = idleColor;
                break;
        }
        indicator.Recolor(indicatorColor);

        Debug.LogFormat("{0} changes state to {1} because: {2}", gameObject.name, currentState, changeReason);
    }

    private void ResetState()
    {
        ChangeState(defaultBehaviour, "What was I doing again?");
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
    
    private bool IsInRange(float range)
    {
        return Vector3.Distance(transform.position, player.transform.position) < range;
    }

    private void OnDrawGizmosSelected()
    {
        //// Display chase radius when selected
        //Gizmos.color = alertColor;
        //Gizmos.DrawWireSphere(transform.position, sightRange);

        //// Display suspicion radius when selected
        //Gizmos.color = suspiciousColor;
        //Gizmos.DrawWireSphere(transform.position, lowSightRange);
        if (currentState == NPCState.Alert)
        {
            Gizmos.color = alertColor;
        }
        else if (currentState == NPCState.Suspicious)
        {
            Gizmos.color = suspiciousColor;
        }
        else
        {
            Gizmos.color = idleColor;
        }
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = postingColor;
        Gizmos.DrawSphere(postLocation, .3f);
        //Gizmos.DrawWireMesh(sight.sightMesh.sharedMesh, sight.sightMesh.transform.position, transform.forward, new Vector3 sightRange);
    }
}
