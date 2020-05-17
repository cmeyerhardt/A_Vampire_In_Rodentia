using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour, IStamina
{
    [Header("Character--")]
    public bool isDead = false;
    public bool isStunned = false;

    public Interactable currentInteractiable = null;

    [Header("Movement")]
    [SerializeField] [Range(0f, 20f)]  public float baseMovementSpeed = 5f;
    [SerializeField] [Range(10f, 20f)] public float maxSpeed = 20f;
    [SerializeField] [Range(0f, 20f)]  public float walkingSpeed = 7f;
    [SerializeField] [Range(0f, 20f)]  public float baseSprintSpeed = 10f;
    [SerializeField] [Range(10f, 30f)] public float dashSpeed = 20f;

    float speedModifier = 1f;
    [HideInInspector] public float currentSpeedModifier;
    float staminaModifier = 0f;
    float staminaLossBase = 5f;
    [SerializeField] public float navMeshDistanceBuffer = 3f;

    [HideInInspector] public FloatingTextSpawner textSpawner = null;
    [HideInInspector] public NavMeshAgent navMeshAgent = null;
    [HideInInspector] public Rigidbody rigidBody = null;
    [HideInInspector] public Colorizer indicator = null;
    [HideInInspector] public Animator animator = null;
    [HideInInspector] public Stamina stamina = null;
    [HideInInspector] public Transform model = null;
    
    [SerializeField] public Transform head = null;

    public virtual void Awake()
    {
        indicator = GetComponentInChildren<Colorizer>();
        textSpawner = GetComponentInChildren<FloatingTextSpawner>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        stamina = GetComponent<Stamina>();
        model = transform.Find("Model");
    }

    public virtual void Start()
    {
        
    }

    public virtual void Update()
    {
        if(isDead) { return; }

        float staminaPerc = stamina.GetStaminaPerc();
        float difference = 0f;
        if (staminaPerc > .5f)
        {
            difference = staminaPerc - .5f;
        }
        else if (staminaPerc < .5f)
        {
            difference = .5f - staminaPerc;
        }
        currentSpeedModifier = speedModifier * (1f + difference);

        if (difference > 0f)
        {
            staminaModifier = -staminaLossBase * (1f + difference);
        }
        else
        {
            staminaModifier = -staminaLossBase;
        }
    }

    public void Die()
    {
        isDead = true;
        navMeshAgent.enabled = false;
        model.rotation = Quaternion.Euler(0f, 0f, -90f);
    }

    public virtual void Stun(float duration)
    {
        //todo -- when stunned, preserve last state/behaviour and disable currentbehaviour script
        //todo -- is stun duration fixed or dependant on other variables?
        isStunned = true;
        stamina.ModifyStamina(-1f);
        textSpawner.SpawnText("Stunned", Color.blue);
        if(duration > 0)
        {
            Invoke("UnStun", duration);
        }
    }

    public virtual void UnStun()
    {
        textSpawner.SpawnText("Stun fades", Color.blue);
        isStunned = false;
    }

    public virtual float GetDeltaModifier()
    {
        return staminaModifier;
    }

    public void MoveToDestination(Vector3 destination, float speedFraction)
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            head.forward = transform.forward;
            navMeshAgent.destination = destination;
            navMeshAgent.speed = baseMovementSpeed * Mathf.Clamp01(speedFraction);
            navMeshAgent.isStopped = false;
        }
    }

    public void MoveInDirection(Vector3 direction, float speed)
    {
        navMeshAgent.Move(direction * speed * Time.deltaTime);
    }

    public void StopMoving()
    {
        if(navMeshAgent.isOnNavMesh)
        {
            MoveToDestination(transform.position, 1f);
            navMeshAgent.isStopped = true;
        }
    }

    public bool IsInRange(Vector3 target, float range)
    {
        return Vector3.Distance(transform.position, target) <= range;
    }
}
