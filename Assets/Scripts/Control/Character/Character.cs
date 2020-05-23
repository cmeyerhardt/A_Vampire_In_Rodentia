using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    [Header("Character--")]
    public bool isDead = false;
    public bool isStunned = false;

    public Interactable currentInteractiable = null;
    [SerializeField] public GameObject objectInHand = null;

    [Header("Movement")]
    [SerializeField] [Range(0f, 20f)]  public float baseMovementSpeed = 5f;
    [SerializeField] [Range(0f, 20f)]  public float sprintSpeed = 10f;
    [SerializeField] [Range(10f, 3000f)] public float dashSpeed = 20f;
    [SerializeField] [Range(45f, 270f)] public float turnSpeed = 100f;
    [SerializeField] [Range(0f, 270f)] public float staminaDrainedWhenStunned = 10f;
    [SerializeField] [Range(0f, 1f)] public float stunResistChance = .1f;
    [SerializeField] [Range(0f, 1f)] public float stunDurationReduction = .1f;
    float navMeshDistanceBuffer = 2f;

    [HideInInspector] public FloatingTextSpawner textSpawner = null;
    [HideInInspector] public NavMeshAgent navMeshAgent = null;
    [HideInInspector] public Rigidbody rigidBody = null;
    [HideInInspector] public Animator animator = null;
    [HideInInspector] public Stamina stamina = null;
    
    [Header("Transform Eeferences")]
    [HideInInspector] public Transform model = null;
    [SerializeField] public Transform head = null;
    [SerializeField] public Transform hand = null;
    [SerializeField] public Collider hitBox = null;

    public virtual void Awake()
    {
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
    }

    public void Die()
    {
        isDead = true;
        navMeshAgent.enabled = false;
        model.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }

    public virtual void Stun(float duration)
    {
        if (Random.Range(0f, 1f) <= stunResistChance)
        {
            isStunned = true;
            stamina.ModifyStamina(staminaDrainedWhenStunned);
            textSpawner.SpawnText("Stunned", Color.blue);

            float _duration = Mathf.Clamp(duration * (1 - stunDurationReduction), 0f, duration);

            if (_duration > 0)
            {
                Invoke("UnStun", _duration);
            }
        }
        else
        {
            UnStun();
        }

    }

    public virtual void UnStun()
    {
        textSpawner.SpawnText("Stun fades", Color.blue);
        isStunned = false;
    }

    public virtual float GetDeltaModifier()
    {
        return 0f;
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

    // Check a position against a given range
    public bool IsInRange(Vector3 target, float range)
    {
        //check distance in relation to feet and head
        return ((transform.position - target).magnitude <= range || (head.position - target).magnitude <= range);
        //return Vector3.Distance(transform.position, target) <= range;
    }

    // Use this method if the range to check is the units stopping distance
    public bool IsInRange(Vector3 target)
    {
        return Mathf.Min((transform.position - target).magnitude, (head.position - target).magnitude) <= GetStoppingDistance();
    }


    public float GetStoppingDistance()
    {
        return navMeshAgent.stoppingDistance + navMeshDistanceBuffer;
    }

    public void DropObject()
    {
        if(objectInHand != null)
        {
            print("Dropping " + objectInHand);
            objectInHand.transform.parent = null;
            Rigidbody droppedObject = objectInHand.GetComponent<Rigidbody>();
            if(droppedObject != null)
            {
                droppedObject.useGravity = true;
                droppedObject.isKinematic = false;
            }
        }
        objectInHand = null;
    }


    public bool PickUpObject(GameObject objectToPickUp)
    {
        if (objectInHand == null)
        {
            if (IsInRange(objectToPickUp.transform.position, GetStoppingDistance()) /*& object not owned by another unit already*/)
            {
                objectInHand = objectToPickUp;
                objectInHand.transform.position = hand.position;
                objectInHand.transform.rotation = transform.rotation;
                objectInHand.transform.parent = hand;
                return true;
            }
            else
            {
                print("Not in range of pickup object");
            }
        }
        else
        {
            print("Already have an object");
        }
        return false;
    }

    public void CancelInteract()
    {
        //print("Canceling " + name + " interact with " + occupant);
        transform.parent = null;

        if (currentInteractiable != null)
        {
            transform.position = currentInteractiable.entryLocation;
            model.transform.rotation = currentInteractiable.entry;
            currentInteractiable.occupied = false;
        }

        currentInteractiable = null;

        //change animation to standing

        navMeshAgent.enabled = true;
        
    }
}
