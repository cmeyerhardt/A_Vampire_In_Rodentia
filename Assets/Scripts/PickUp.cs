using UnityEngine;

public class PickUp : MonoBehaviour, IRaycast
{
    Character owner = null;
    Rigidbody rigidBody = null;
    Collider thisCollider = null;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        thisCollider = GetComponentInChildren<Collider>();
    }

    public CursorType GetCursorType()
    {
        return CursorType.PickUp;
    }

    private void Update()
    {
        if(owner == null || owner.isDead)
        {
            BePickedUp(null, false);
        }
    }

    public bool HandleRaycast(PlayerController playerController)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (playerController.IsInRange(transform.position, playerController.GetStoppingDistance()))
            {
                if (playerController.objectInHand != null)
                {
                    playerController.DropObject(false);
                }
                BePickedUp(playerController, true);
            }
            else
            {
                playerController.textSpawner.SpawnText("Out of Range", true, Color.red);
            }
        }
        return false;
    }

    public void BePickedUp(Character newOwner, bool pickedUp)
    {
        if (owner != null)
        {
            //Check if owner is changing
            if (owner == newOwner) { return; }

            // if owner is changing, check if the new owner is the player
            if (newOwner.GetComponent<PlayerController>() != null)
            {
                owner.DropObject(true);
            }
            else
            {
                owner.DropObject(false);
            }
        }

        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        if (rigidBody != null)
        {
            thisCollider.isTrigger = pickedUp;
            rigidBody.isKinematic = pickedUp;
            rigidBody.useGravity = !pickedUp;
        }

        owner = newOwner;
        if(newOwner != null)
        {
            newOwner.PickUpObject(this);
        }
    }
}
