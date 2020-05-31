using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Siren : GoToObject
{
    [Header("Siren--")]
    [SerializeField] float maxSeekDistance = 40f;
    Protector protector = null;
    Vector3 playerPosition = new Vector3();
    [SerializeField] float sirenNoiseLevel = 10f;

    private new void OnEnable()
    {
        playerPosition = ai.player.transform.position;
        ai.player.makeSoundEvent.Invoke(sirenNoiseLevel);
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
            doneEvent.Invoke(this);
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
        if(protector == null) { return; }
        if(ai.IsInRange(protector.transform.position))
        {
            if(protector.behaviourMap.ContainsKey("GoToPlayer"))
            {
                ((GoToLocation)protector.behaviourMap["GoToPlayer"]).nullableLocation = playerPosition;
            }
            protector.currentState = NPCState.Suspicious;
            doneEvent.Invoke(this);
        }
    }

}
