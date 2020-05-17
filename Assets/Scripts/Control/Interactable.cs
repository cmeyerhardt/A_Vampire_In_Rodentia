using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour, IRaycast
{
    [SerializeField] public bool allowPlayer = true;
    public bool occupied = false;
    public Character occupant;
    [HideInInspector] public Transform useTransform;

    // Cache
    Vector3 entryLocation = new Vector3();
    Quaternion entry;
    Color debugColor = Color.magenta;

    public virtual void Awake()
    {
        useTransform = transform.Find("UseTransform");
    }

    public CursorType GetCursorType()
    {
        return CursorType.Hide;
    }

    public bool HandleRaycast(PlayerController playerController)
    {
        if ((!occupied && allowPlayer) && playerController.playerState != PlayerState.Hiding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (playerController.IsInRange(transform.position, playerController.navMeshAgent.stoppingDistance))
                {
                    Interact(playerController);
                    playerController.playerState = PlayerState.Interacting;
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

    public virtual void Interact(Character occupant)
    {
        print(gameObject.name + " now interacting with " + occupant.name);
        occupied = true;
        this.occupant = occupant;
        occupant.navMeshAgent.enabled = false;

        entry = occupant.transform.rotation;
        entryLocation = occupant.transform.position;

        occupant.transform.position = useTransform.position;
        occupant.model.transform.rotation = useTransform.rotation;
        occupant.transform.parent = transform;

        occupant.currentInteractiable = this;
    }

    public virtual void CancelInteract()
    {
        print("Canceling " + name + " interact with " + occupant);
        occupant.transform.parent = null;


        occupant.transform.position = entryLocation;
        occupant.model.transform.rotation = entry;

        occupant.currentInteractiable = null;

        //change animation to standing

        occupant.navMeshAgent.enabled = true;
        occupied = false;
    }

    private void OnDrawGizmos()
    {
        if (occupied)
        {
            Gizmos.color = debugColor;
            Gizmos.DrawSphere(entryLocation, .3f);
        }
    }
}
