using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerVolume : MonoBehaviour
{
    public UnityEvent triggerEvent;
    bool oneTimeOnly = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            triggerEvent.Invoke();
            if(oneTimeOnly)
            {
                DestroySelf();
            }
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
