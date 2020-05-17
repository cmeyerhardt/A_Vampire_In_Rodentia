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
        availableBehaviours = "Attack, GoToLocation, GoToObject, Patrol, Pickup, Siren, Wait";
#endif
        base.Update();
    }
}
