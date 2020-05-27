using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    [Header("Character--")]
    public bool isDead = false;
    public bool isStunned = false;

    public Interactable currentInteractiable = null;
    [SerializeField] public PickUp objectInHand = null;

    [Header("Movement")]
    [SerializeField] [Tooltip("Base movement speed for this unit")]
    [Range(0f, 20f)]  public float baseMovementSpeed = 5f;

    [SerializeField] [Tooltip("Override for max movement speed for this unit.\nWill override any other movement speed modifiers")]
    [Range(0f, 40f)]  public float maxMovementSpeed = 20f;

    [SerializeField] [Tooltip("Add to navMeshAgent.stoppingDistance when checking range to destination. Helps agent reach destinations that are wider than stopping distance")]
    [Range(2f, 5f)] float navMeshDistanceBuffer = 2.5f;

    public bool walking = false;

    [Header("Stunning")]
    [SerializeField] [Range(0f, 10f)] public float outgoingStunRange = 3f;
    [SerializeField] [Range(0f, 50f)] public float outgoingStunDuration = 3f;
    [SerializeField] [Range(0f, 100f)] public float staminaDrainWhenImStunned = 10f;
    [SerializeField] [Range(0f, 1f)] public float myStunResistChance = .1f;
    [SerializeField] [Range(0f, 1f)] public float myStunDurationReduction = .1f;

    [Header("Audio")]
    [SerializeField] AudioClip stunSound = null;
    [SerializeField] AudioClip footsteps = null;
    [SerializeField] [Range(0f, 2f)] public float footStepInterval = 1f;
    [HideInInspector] public float footstepCounter = 0f;
    Vector3 currentDestination = new Vector3();

    [HideInInspector] public FloatingTextSpawner textSpawner = null;
    [HideInInspector] public NavMeshAgent navMeshAgent = null;
    [HideInInspector] public Rigidbody rigidBody = null;
    [HideInInspector] public Animator animator = null;
    [HideInInspector] public Stamina stamina = null;
    [HideInInspector] public AudioSource audioSource = null;

    
    [Header("Transform References")]
    [SerializeField] public Transform head = null;
    [SerializeField] public Transform hand = null;
    [SerializeField] public Collider hitBox = null;
    [HideInInspector] public Transform model = null;

    public virtual void Awake()
    {
        textSpawner = GetComponentInChildren<FloatingTextSpawner>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        stamina = GetComponent<Stamina>();
        model = transform.Find("Model");
        audioSource = GetComponent<AudioSource>();
    }

    public virtual void Start()
    {
    }

    public virtual void Update()
    {
        if(isDead) { return; }
        if(walking)
        {
            MakeMovementSounds(footsteps);
        }
        if (walking && IsInRange(currentDestination))
        {
            walking = false;
        }
    }

    public void AnimationEventMakeFootstepsSoundEffect()
    {
        MakeMovementSounds(footsteps);
    }

    public virtual void MakeMovementSounds(AudioClip clip)
    {
        if (footstepCounter > footStepInterval)
        {
            PlaySoundEffect(clip);
            footstepCounter = 0f;
        }
        else
        {
            footstepCounter += Time.deltaTime;
        }
    }

    public virtual void Die()
    {
        isDead = true;

        //todo -- should dead bodies block movement?
        navMeshAgent.enabled = false;

        //update animator

        model.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public virtual void StunTarget(Character target)
    {
        PlaySoundEffect(stunSound);
        print(gameObject.name +" stuns "+ target.name);
        target.BecomeStunned(outgoingStunDuration);
    }

    public virtual void BecomeStunned(float duration)
    {
        if (Random.Range(0f, 1f) <= myStunResistChance)
        {
            isStunned = true;
            stamina.ModifyStamina(staminaDrainWhenImStunned);
            textSpawner.SpawnText("Stunned", Color.blue);

            float _duration = Mathf.Clamp(duration * (1 - myStunDurationReduction), 0f, duration);

            if (_duration > 0)
            {
                Invoke("UnStun", _duration);
            }
        }
        else
        {
            BecomeUnStunned();
        }
    }

    public virtual void BecomeUnStunned()
    {
        textSpawner.SpawnText("Stun Fades", Color.blue);
        isStunned = false;
    }

    public void MoveToDestination(Vector3 destination, float speedFraction)
    {
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            walking = true;
            head.forward = transform.forward;
            currentDestination = destination;
            navMeshAgent.destination = destination;
            navMeshAgent.speed = Mathf.Clamp(baseMovementSpeed * Mathf.Clamp01(speedFraction), baseMovementSpeed, maxMovementSpeed);
            navMeshAgent.isStopped = false;
        }
    }

    //public void MoveInDirection(Vector3 direction, float speed)
    //{
    //    navMeshAgent.Move(direction * speed * Time.deltaTime);
    //}

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

    public virtual void DropObject(bool objectTaken)
    {
        if(objectInHand != null)
        {
            //print("Dropping " + objectInHand);
            objectInHand.transform.parent = null;
        }
        objectInHand = null;
    }


    public bool PickUpObject(PickUp objectToPickUp)
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
