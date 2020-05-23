using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    float projectileSpeed = 20f;
    [SerializeField] GameObject[] destroyOnHit = null;
    [SerializeField] GameObject hitFX = null;
    public UnityEvent projectileHitEvent;
    GameObject originator = null;

    bool stopped = false;
    bool hit = false;
    bool playerDamaged = false;

    Health player;
    bool initialized = false;
    float lifeTime = 12f;

    Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    Vector3 point = new Vector3();

    public void Initialize(Health player, GameObject originator)
    {
        this.originator = originator;
        this.player = player;
        Collider hitBox = player.GetComponent<Character>().hitBox;
        //hitBox.ClosestPoint
        transform.LookAt(hitBox.ClosestPointOnBounds(transform.position));
        Destroy(gameObject, lifeTime);
        initialized = true; 
    }

    private void OnDrawGizmos()
    {
        if (initialized)
        {
            Gizmos.DrawSphere(point, .2f);
        }
    }

    void Update()
    {
        if(!initialized) { return; }

        if(!stopped)
        {
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!other.GetComponent<Health>()) { return; }
        if (player.isDead) { return; }

        if(!other.isTrigger && other.gameObject != originator)
        {
            stopped = true;
            transform.parent = other.transform;
        }


        if (other.GetComponent<Health>() == player && !playerDamaged)
        {
            playerDamaged = true;
            projectileHitEvent.Invoke();
            transform.parent = player.GetComponent<Character>().model;
        }

        if (hitFX && !hit)
        {
            hit = true;
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
