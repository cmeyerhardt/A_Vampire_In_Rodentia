using UnityEngine;

public class Protector : AIController, IRaycast
{
    public CursorType GetCursorType()
    {
        return CursorType.Guard;
    }

    public bool HandleRaycast(PlayerController playerController)
    {
        if(!isDead)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _ = playerController.CheckStunConditions(this);
            }
            return true;
        }
        return false;
    }

    private new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
#if UNITY_EDITOR
        availableBehaviours = "Attack,GoToLocation, GoToObject, PickUpObject, DropObject, Patrol, Siren, Wait";
#endif
        base.Update();
    }

    public override void PlayerHeard(bool heard)
    {
        base.PlayerHeard(heard);
        if (currentBehaviour != "Return") //todo: keep this? prevents ai state change due to hearing player while returning to tether
        {
            //Debug.Log("Next...");
            switch (currentState)
            {
                case NPCState.None:
                case NPCState.Default:
                    //Debug.Log(transform.gameObject + " becoming suspicious");
                    currentState = NPCState.Suspicious;
                    break;
                case NPCState.Suspicious:
                    lastState = NPCState.None;
                    break;
                default:
                    break;
            }
        }
    }
}
