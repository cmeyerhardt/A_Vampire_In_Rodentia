using UnityEngine;
using UnityEngine.Events;

public class FeedingVictim : MonoBehaviour, IRaycast
{
    [HideInInspector] public BoolEvent fedOnEvent;
    [HideInInspector] public FloatEvent feedingOn;
    [HideInInspector] public BoolEvent resistEvent;

    //[SerializeField] float recoveryDelay = 3f;
    bool beingFedOn = false;
    [SerializeField] float healthLossOnFeed = 5f;
    [SerializeField] float chanceToResistFeed = .1f;

    Villager villager = null;

    private void Awake()
    {
        villager = GetComponent<Villager>();
        feedingOn.AddListener(villager.health.ModifyHealth);
        feedingOn.AddListener(villager.ChangeFurColor);
    }

    private void Update()
    {
        if(beingFedOn)
        {
            villager.currentState = NPCState.Incapacitated;
        }
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
            beingFedOn = true;
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
        beingFedOn = false;
        GetComponent<AIController>().BecomeUnStunned();
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
