using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Siren : GoToObject
{
    [Header("Siren--")]
    [SerializeField] float maxSeekDistance = 40f;
    Protector protector = null;

    private new void OnEnable()
    {
        Protector[] protectors = FindObjectsOfType<Protector>();

        if(protectors.Length > 0)
        {
            foreach (Protector protector in protectors)
            {
                if ((transform.position - protector.transform.position).magnitude < maxSeekDistance)
                {
                    objectReference = protector.gameObject;
                    this.protector = protector;
                    tasks.Add("AlertGuard");
                    break;
                }
            }
        }

        if (protector == null)
        {
            print("No guards!");
        }

        base.OnEnable();
    }

    public new void Start()
    {
        base.Start();
    }

    public new void Update()
    {
        base.Update();
        if(ai.IsInRange(protector.transform.position))
        {
            protector.currentState = NPCState.Alert;
            doneEvent.Invoke(this);
        }
    }

}
