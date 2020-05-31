using UnityEngine;
using UnityEngine.Events;

public class DelayedAction : MonoBehaviour
{
    [SerializeField] float delayInSeconds = 2f;
    public UnityEvent timerFinished;

    private void Start()
    {
        Invoke("InvokeEvent", delayInSeconds);
        Destroy(this, delayInSeconds + 2f);
    }

    private void InvokeEvent()
    {
        timerFinished.Invoke();
    }
}
