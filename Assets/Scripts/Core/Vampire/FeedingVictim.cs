using UnityEngine;
using UnityEngine.Events;

public class FeedingVictim : MonoBehaviour, IRaycast
{
    [HideInInspector] public BoolEvent fedOnEvent;
    [HideInInspector] public FloatEvent feedingOn;
    [HideInInspector] public BoolEvent resistEvent;
    
    //[SerializeField] float recoveryDelay = 3f;
    
    [SerializeField] float healthLossOnFeed = 5f;
    [SerializeField] float chanceToResistFeed = .1f;

    private void Awake()
    {
        feedingOn.AddListener(GetComponent<Health>().ModifyHealth);
        feedingOn.AddListener(GetComponent<Villager>().ChangeFurColor);
    }

    public float GetFedValue()
    {
        return healthLossOnFeed;
    }

    public void FeedOn()
    {
        feedingOn.Invoke(-GetFedValue());
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
