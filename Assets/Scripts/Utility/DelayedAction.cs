using UnityEngine;
using UnityEngine.Events;

public class DelayedAction : MonoBehaviour
{
    [SerializeField] float delayInSeconds = 2f;
    public UnityEvent timerFinished;

    void Update()
    {
        if(delayInSeconds > 0f)
        {
            delayInSeconds -= Time.deltaTime;
        }
        else
        {
            timerFinished.Invoke();
            Destroy(this, 1f);
        }
    }
}
