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
    }

    private void Start()
    {
        feedingOn.AddListener(villager.health.ModifyHealth);
        feedingOn.AddListener(villager.GetComponent<MaterialColors>().ChangeFurColor);
    }

    private void Update()
    {
        if(beingFedOn)
        {
            villager.isStunned = true;
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
            print("unsuccessful");
            succeeded = false;
            CancelBeingFedOn();
        }
        else
        {
            //print("successful");
            beingFedOn = true;
            succeeded = true;
            fedOnEvent.Invoke(true);
        }
    }

    public void CancelBeingFedOn()
    {
        //print(gameObject.name + " canceling feeding");
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
            playerController.target = gameObject;
            if (Input.GetMouseButtonDown(0))
            {

                playerController.CheckFeedingConditions(this);
            }
            return true;
        }
        return false;
    }
}
