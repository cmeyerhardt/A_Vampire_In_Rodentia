using UnityEngine;
using System.Collections;

public class Feeder : MonoBehaviour
{
    public FloatEvent feedEvent;
    [Header("How close does the player need to be to feed?")]
    [SerializeField] float feedingDistance = 3f;

    [Header("How often does the player gain stamina?")]
    [SerializeField] float feedInterval = 2f;

    [SerializeField] FloatingTextSpawner textSpawner = null;

    // Cache
    float feedCounter = 0f;
    FeedingVictim currentVictim = null;

    private void Update()
    {
        if(currentVictim != null)
        {
            if (feedCounter >= feedInterval)
            {
                feedCounter = 0f;
                Feed();
            }
            feedCounter += Time.deltaTime;
        }
    }
    
    public void AssignVictim(FeedingVictim victim)
    {

        currentVictim = victim;
        //Feeding Animation
    }

    private void Feed()
    {
        feedEvent.Invoke(currentVictim.GetFedValue());
        currentVictim.FedOn();
    }

    public void CancelFeeding()
    {
        textSpawner.SpawnText("-Cancel Feeding-", Color.yellow);
        currentVictim.CancelBeingFedOn();
        currentVictim = null;
        feedCounter = 0f;
    }

    public float GetFeedingDistance()
    {
        return feedingDistance;
    }
}



