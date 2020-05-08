using UnityEngine;
using UnityEngine.Events;

public class FeedingVictim : MonoBehaviour
{
    [HideInInspector] public BoolEvent fedOnEvent;
    [HideInInspector] public FloatEvent feedEvent;
    
    [Header("How long will it take the NPC to be able to react? (sec.)")]
    [SerializeField] float recoveryDelay = 3f;
    
    [Header("How much stamina will the player gain per feeding?")]
    [SerializeField] float staminaFedValue = 5f;

    public float GetFedValue()
    {
        return staminaFedValue;
    }

    public void FedOn()
    {
        feedEvent.Invoke(-staminaFedValue);
    }

    public void BeginFedOn()
    {
        fedOnEvent.Invoke(true);
    }

    public void CancelBeingFedOn()
    {
        Invoke("ReleaseState", recoveryDelay);
    }

    private void ReleaseState()
    {
        fedOnEvent.Invoke(false);
    }
}
