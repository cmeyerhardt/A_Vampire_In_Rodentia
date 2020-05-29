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
        if(target.isDead) { return; }
        if(!stopped)
        {
            if (!other.isTrigger && other.gameObject != originator)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
                transform.parent = other.transform;
            }

            if (target != null && target == player && !player.isDead)
            {
                stopped = true;
                if (!playerDamaged)
                {
                    playerDamaged = true;
                    projectileHitEvent.Invoke(true);
                    transform.parent = player.GetComponent<Character>().model;
                }
            }
            else
            {
                stopped = true;
                originator.PlaySoundEffect(hitSoundObject, hitSoundObjectMaxVolume);
                projectileHitEvent.Invoke(false);
            }

            if (hitFX)
            {
                Vector3 closestPoint = other.ClosestPoint(transform.position);
                GameObject impactObj = Instantiate(hitFX);

                impactObj.transform.position = closestPoint;
                impactObj.transform.rotation = transform.rotation;

                Destroy(impactObj, 1f);
            }

            foreach (GameObject toDestroy in destroyOnHit)
            {
                Destroy(toDestroy);
            }
        }
    }
}
