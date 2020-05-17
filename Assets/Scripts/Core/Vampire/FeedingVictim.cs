using UnityEngine;
using UnityEngine.Events;

public class FeedingVictim : MonoBehaviour, IRaycast
{
    [HideInInspector] public BoolEvent fedOnEvent;
    [HideInInspector] public FloatEvent feedEvent;
    [HideInInspector] public BoolEvent resistEvent;
    
    //[SerializeField] float recoveryDelay = 3f;
    
    [SerializeField] float healthLossOnFeed = 5f;
    [SerializeField] float chanceToResistFeed = .1f;

    private void Awake()
    {
        feedEvent.AddListener(GetComponent<Health>().ModifyHealth);
    }

    public float GetFedValue()
    {
        return healthLossOnFeed;
    }

    public void FeedOn()
    {
        feedEvent.Invoke(-healthLossOnFeed);
    }

    public void BeginFeeding(out bool succeeded)
    {
        if(Random.Range(0f,1f) <= chanceToResistFeed)
        {
            succeeded = false;
            CancelBeingFedOn();
        }
        else
        {
            succeeded = true;
            fedOnEvent.Invoke(true);
        }
    }

    public void CancelBeingFedOn()
    {
        GetComponent<AIController>().UnStun();
        fedOnEvent.Invoke(false);
    }
    
    public CursorType GetCursorType()
    {
        return CursorType.Victim;
    }

    public bool HandleRaycast(PlayerController playerController)
    {
        if(!GetComponent<Character>().isDead && playerController.playerState != PlayerState.Feeding)
        {
            if(Input.GetMouseButtonDown(0))
            {
                playerController.CheckFeedingConditions(this);
            }
            return true;
        }
        return false;
    }
}
