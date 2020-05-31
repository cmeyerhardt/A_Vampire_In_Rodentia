using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : HidingPlace, IRaycast
{
    public new CursorType GetCursorType()
    {
        return CursorType.Hide;
    }

    public new bool HandleRaycast(PlayerController playerController)
    {
        if ((!occupied && allowPlayer) && playerController.playerState != PlayerState.Hiding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (playerController.IsInRange(transform.position, playerController.navMeshAgent.stoppingDistance))
                {
                    Interact(playerController);
                    playerController.SetState(PlayerState.Hiding);
                }
                else
                {
                    playerController.textSpawner.SpawnText("Out of Range", true, Color.red);
                }
            }
            return true;
        }
        return false;
    }

    public new void Awake()
    {
        base.Awake();
    }

    //public override void Interact(Character occupant)
    //{
    //    base.Interact(occupant);
    //}

    //public override void CancelInteract()
    //{
    //    base.CancelInteract();
    //}
}
