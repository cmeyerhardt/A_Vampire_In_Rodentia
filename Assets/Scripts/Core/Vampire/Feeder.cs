using UnityEngine;

public class Feeder : MonoBehaviour
{
    public FloatEvent feedEvent;
    [SerializeField] [Tooltip("sec.")][Range(0f,10f)]float feedIntervalWhileLatched = 2f;
    [SerializeField] [Tooltip("sec.")][Range(0f,10f)]float timeToWaitToAvoidInturruption = 1f;
    [SerializeField] [Range(0f,10f)]float feedInturruptionCost = 1f;
    [SerializeField] [Range(0f,1f)] float healthModRatio = .5f;

    // Cache
    float feedCounter = 0f;
    float totalTimeFeeding = 0f;
    FeedingVictim currentVictim = null;
    Health health = null;
    Stamina stamina = null;

    FloatingTextSpawner textSpawner = null;

    private void Awake()
    {
        health = GetComponent<Health>();
        stamina = GetComponent<Stamina>();
        textSpawner = GetComponentInChildren<FloatingTextSpawner>();
    }

    private void Start()
    {
        //feedEvent.AddListener(GetComponent<Stamina>().ModifyStamina);
    }

    private void Update()
    {
        if(currentVictim != null)
        {
            if (feedCounter >= feedIntervalWhileLatched)
            {
                feedCounter = 0f;
                Feed();
            }
            feedCounter += Time.deltaTime;
            totalTimeFeeding += Time.deltaTime;
        }
    }
    
    public void AssignVictim(FeedingVictim victim)
    {
        currentVictim = victim;
        //Trigger Feeding Animation
    }

    private void Feed()
    {
        if(!currentVictim.GetComponent<Health>().isDead)
        {
            //feedEvent.Invoke(currentVictim.GetFedValue());
            currentVictim.FeedOn();
            health.ModifyHealth(currentVictim.GetFedValue() * healthModRatio);
            stamina.ModifyStamina(currentVictim.GetFedValue());
        }
        else
        {
            CancelFeeding();
        }
    }

    public void CancelFeeding()
    {
        //Check if cancel early
        if (totalTimeFeeding < timeToWaitToAvoidInturruption)
        {
            textSpawner.SpawnText("Penalty", new Color(1f, .5f, 0f));
            stamina.ModifyStamina(-feedInturruptionCost);
            //feedEvent.Invoke(-feedInturruptionCost);
        }

        if (currentVictim != null)
        {
            currentVictim.CancelBeingFedOn();
        }
        
        currentVictim = null;

        feedCounter = 0f;
        totalTimeFeeding = 0f;
    }
}



