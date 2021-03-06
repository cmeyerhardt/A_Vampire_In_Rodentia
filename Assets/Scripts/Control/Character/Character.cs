﻿using UnityEngine;
using UnityEngine.AI;

public class Character : MonoBehaviour
{
    [Header("Character--")]
    public bool isDead = false;
    public bool isStunned = false;

    public Interactable currentInteractiable = null;
    [SerializeField] public PickUp objectInHand = null;
    Transform originalParent = null;
    [Range(1f, 10f)] public float destroyOnDeathDelay = 5f;

    [Header("Movement")]
    [SerializeField] [Tooltip("Base movement speed for this unit")]
    [Range(0f, 20f)]  public float baseMovementSpeed = 5f;
    [SerializeField] [Tooltip("Override for max movement speed for this unit.\nThis will override any other movement speed modifiers")]
    [Range(0f, 40f)]  public float maxMovementSpeed = 20f;
    [SerializeField] [Tooltip("Add to navMeshAgent.stoppingDistance when checking range to destination. Helps agent reach destinations that are wider than stopping distance")]
    [Range(2f, 15f)] float navMeshDistanceBuffer = 2.5f;

    public bool walking = false;

    [Header("Stunning")]
    [SerializeField] [Range(0f, 10f)] public float outgoingStunRange = 3f;
    [SerializeField] [Range(0f, 50f)] public float outgoingStunDuration = 3f;
    [SerializeField] [Range(0f, 100f)] public float staminaDrainWhenImStunned = 10f;
    [HideInInspector] public float myStunResistChance = 0f;
    [HideInInspector] public float myStunDurationReduction = 0f;

    [Header("Audio")]
    [SerializeField] AudioClip dyingSound = null;
    [SerializeField] [Range(0, 1)] float dyingSoundMaxVolume = 1f;
    [SerializeField] public bool useSecondaryAudioSourceDyingSound = false;
    [SerializeField] AudioClip stunSound = null;
    [SerializeField] [Range(0f, 1f)] public float stunSoundVolume = 1f;
    [SerializeField] public bool useSecondaryAudioSourceStunSound = false;
    [SerializeField] AudioClip footsteps = null;
    [SerializeField] [Range(0f, 1f)] public float footStepsVolume = 1f;
    [SerializeField] public bool useSecondaryAudioSourceFootstepsSound = false;

    //[SerializeField] [Range(0f, 2f)] public float footStepInterval = 1f;
    //[HideInInspector] public float footstepCounter = 0f;
    Vector3 currentDestination = new Vector3();

    [Header("References")]
    [HideInInspector] public Animator animator = null;
    [SerializeField] public AudioSource primaryAudioSource = null;
    [SerializeField] public AudioSource secondaryAudioSource = null;
    [HideInInspector] public FloatingTextSpawner textSpawner = null;
    [HideInInspector] public NavMeshAgent navMeshAgent = null;
    [HideInInspector] public Rigidbody rigidBody = null;
    [HideInInspector] public Stamina stamina = null;

    [Header("Transform References")]
    [SerializeField] public Transform head = null;
    [SerializeField] public Transform hand = null;
    [SerializeField] public Collider hitBox = null;
    [HideInInspector] public Transform model = null;

    public virtual void Awake()
    {
        originalParent = transform.parent;
        textSpawner = GetComponentInChildren<FloatingTextSpawner>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        rigidBody = GetComponent<Rigidbody>();

        stamina = GetComponent<Stamina>();
        model = transform.Find("Model");
        animator = model.GetComponentInChildren<Animator>();
        primaryAudioSource = GetComponent<AudioSource>();
        secondaryAudioSource = transform.Find("SecondaryAudioSource").GetComponent<AudioSource>();
    }

    public virtual void Start()
    {
        SetAnimatorSpeed(0);
    }

    private void SetAnimatorSpeed(float speed)
    {
        //print("Setting animator speed for " + gameObject.name + " to " + speed);

        animator.SetInteger("State", 0);
        animator.SetFloat("Speed", speed);
    }

    public virtual void Update()
    {
        if(isDead) { return; }

        if (walking && IsInRange(currentDestination))
        {
            //print(gameObject.name + "At destination");
            walking = false;
            //StopMoving();
            SetAnimatorSpeed(0);
        }
    }


    /*
     * ANIMATION
     */
    public virtual void AnimationEventMakeFootstepsSoundEffect()
    {
        PlaySoundEffect(footsteps, footStepsVolume, useSecondaryAudioSourceFootstepsSound);
    }

    public virtual void AnimationEventResetAttack()
    {
        animator.SetInteger("State", 0);
    }

    public virtual void AnimationEventHit()
    {
        PlaySoundEffect(stunSound, stunSoundVolume, useSecondaryAudioSourceStunSound);
    }



    /*
     * SOUND FX
     */
    public virtual void MakeMovementSounds(AudioClip clip, float volume, bool secondary)
    {
        PlaySoundEffect(clip, volume, secondary);
    }

    public virtual void PlaySoundEffect(AudioClip clip, float volumeScale, bool secondary)
    {
        if(clip != null)
        {
            //print("Playing clip " + clip.name + " at volume " + volumeScale);
            if (secondary)
            {
                secondaryAudioSource.Stop();
                secondaryAudioSource.clip = clip;
                secondaryAudioSource.volume = volumeScale;
                secondaryAudioSource.PlayOneShot(clip, volumeScale);
            }
            else
            {
                primaryAudioSource.Stop();
                primaryAudioSource.clip = clip;
                primaryAudioSource.volume = volumeScale;
                primaryAudioSource.PlayOneShot(clip, volumeScale);
            }
        }
    }



    /*
     * STUN
     */
    public virtual void StunTarget(Character target, bool secondary, float volume = 1f, AudioClip clip = null)
    {
        animator.SetInteger("State", 1);
        if (clip == null)
        {
            PlaySoundEffect(stunSound, volume, secondary);
        }
        else
        {
            PlaySoundEffect(clip, volume, secondary);
        }

        if (Random.Range(0f, 1f) > target.myStunResistChance)
        {
            //print(gameObject.name + " stuns " + target.name);

            target.BecomeStunned(outgoingStunDuration);
        }
    }

    public virtual void BecomeStunned(float duration)
    {
        //isStunned = true;
        BecomeStunned();
        SetAnimatorSpeed(0);
        StopMoving();
        //textSpawner.SpawnText("Stunned", Color.blue);
        stamina.ModifyStamina(-staminaDrainWhenImStunned);


        float _duration = Mathf.Clamp(duration * (1 - myStunDurationReduction), 0f, duration);

        if (_duration > 0)
        {
            Invoke("BecomeUnStunned", _duration);
        }
    }

    public virtual void BecomeStunned()
    {
        isStunned = true;
        textSpawner.SpawnText("Stunned", true, Color.blue);
        animator.SetInteger("State", 5);
    }

    public virtual void BecomeUnStunned()
    {
        textSpawner.SpawnText("Stun Fades", true, Color.blue);
        isStunned = false;
        animator.SetInteger("State", 0);
    }




    /*
     * DEATH
     */
    public virtual void Die()
    {
        if (dyingSound != null && !isDead)
        {
            PlaySoundEffect(dyingSound, dyingSoundMaxVolume, useSecondaryAudioSourceDyingSound);
        }
        isDead = true;

        navMeshAgent.enabled = false;

        //update animator
        animator.StopPlayback();
        model.rotation = Quaternion.Euler(-90f, 0f, 0f);

        if(gameObject.tag != "Player")
        {
            Destroy(gameObject, destroyOnDeathDelay);
        }
    }

    /*
    * MOVEMENT
    */
    public void MoveToDestination(Vector3 destination, float speedFraction)
    {
        if(speedFraction <= 0)
        {
            return;
        }
        if (navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            walking = true;
            head.forward = transform.forward;
            currentDestination = destination;
            navMeshAgent.destination = destination;
            float _speed = Mathf.Clamp(baseMovementSpeed * speedFraction, baseMovementSpeed, maxMovementSpeed);
            //print(gameObject.name + " speed " + _speed);
            navMeshAgent.speed = _speed;
            navMeshAgent.isStopped = false;

            //print(gameObject.name + " going to " + destination);
            SetAnimatorSpeed(_speed);
        }
    }

    //public void MoveInDirection(Vector3 direction, float speed)
    //{
    //    navMeshAgent.Move(direction * speed * Time.deltaTime);
    //}

    public void StopMoving()
    {
        animator.SetInteger("State", 0);
        animator.SetFloat("Speed", 0);
        if (navMeshAgent.isOnNavMesh)
        {
            //MoveToDestination(transform.position, 0);
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







    /*
    * INTERACTION
    */
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
        //transform.parent = null;

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
