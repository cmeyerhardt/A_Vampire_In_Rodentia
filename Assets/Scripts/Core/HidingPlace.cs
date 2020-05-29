using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//todo -- use inheritance for beds, hiding places

public class HidingPlace : Interactable, IRaycast
{
    public new void Awake()
    {
        base.Awake();
    }

    public new CursorType GetCursorType()
    {
        return CursorType.Hide;
    }

    public new bool HandleRaycast(PlayerController playerController)
    {
        if((!occupied && allowPlayer) && playerController.playerState != PlayerState.Hiding)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if (playerController.IsInRange(GetComponent<Collider>().ClosestPointOnBounds(transform.position), playerController.GetStoppingDistance()))
                {
                    Interact(playerController);
                    playerController.SetState(PlayerState.Hiding);
                }
                else
                {
                    playerController.textSpawner.SpawnText("Out of Range");
                }
            }
            return true;
        }
        return false;
    }

    public override void Interact(Character occupant)
    {
        base.Interact(occupant);
    }

    public override void CancelInteract()
    {
        base.CancelInteract();
    }
}
