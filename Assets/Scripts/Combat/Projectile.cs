using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [Header("Configure")]
    [SerializeField] [Range(0f,50f)]float projectileSpeed = 20f;
    [SerializeField] [Range(0f,50f)] float lifeTime = 12f;
    [SerializeField] GameObject[] destroyOnHit = null;

    [Header("Hit FX")]
    [SerializeField] AudioClip hitSoundObject = null;
    [SerializeField] [Range(0f, 1f)] float hitSoundObjectMaxVolume = 1f;
    [SerializeField] public bool useSecondaryAudioSourceHitObjectSound = false;
    [SerializeField] GameObject hitFX = null;
    public BoolEvent projectileHitEvent;
    
    // Cache
    Character originator = null;
    Health player = null;
    Rigidbody rb = null;

    bool stopped = true;
    //bool hit = false;
    bool playerDamaged = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Health player, Character originator)
    {
        this.originator = originator;
        this.player = player;
        Collider hitBox = player.GetComponent<Character>().hitBox;
        transform.LookAt(hitBox.ClosestPointOnBounds(transform.position));
        Destroy(gameObject, lifeTime);
        stopped = false;
    }

    void Update()
    {
        if(!stopped)
        {
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Health target = other.GetComponent<Health>();
        if(target == null) { return; }
        if(target.isDead) { return; }
        if(!stopped && !other.isTrigger)
        {
            //if (other.gameObject != originator)
            //{
            //    rb.useGravity = false;
            //    rb.isKinematic = true;
            //    transform.parent = other.transform;
            //    stopped = true;
            //}

            if (target != null && target == player/* && !player.isDead*/)
            {
                if (!playerDamaged)
                {
                    playerDamaged = true;
                    rb.useGravity = false;
                    rb.isKinematic = true;
                    stopped = true;
                    projectileHitEvent.Invoke(true);
                    transform.parent = player.GetComponent<Character>().model;
                }
            }
            else if (other.gameObject != originator)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                stopped = true;
                projectileHitEvent.Invoke(false);
                transform.parent = other.transform;
                originator.PlaySoundEffect(hitSoundObject, hitSoundObjectMaxVolume, useSecondaryAudioSourceHitObjectSound);
            }

            try
            {
                if (hitFX != null)
                {
                    Vector3 closestPoint = other.ClosestPoint(transform.position);
                    GameObject hitPFX = Instantiate(hitFX);

                    hitPFX.transform.position = closestPoint;
                    hitPFX.transform.rotation = transform.rotation;

                    Destroy(hitPFX, 1f);
                }
            }
            catch { }


            //foreach (GameObject toDestroy in destroyOnHit)
            //{
            //    Destroy(toDestroy);
            //}
        }
    }
}
