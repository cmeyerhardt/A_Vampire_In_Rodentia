using UnityEngine;

public class PickUp : MonoBehaviour, IRaycast
{
    public CursorType GetCursorType()
    {
        return CursorType.PickUp;
    }

    public bool HandleRaycast(PlayerController playerController)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (playerController.IsInRange(transform.position, playerController.GetStoppingDistance()))
            {
                if (playerController.objectInHand != null)
                {
                    playerController.DropObject();
                }
                playerController.PickUpObject(gameObject);
            }
            else
            {
                playerController.textSpawner.SpawnText("Out of Range");
            }
        }
        return false;
    }
}
