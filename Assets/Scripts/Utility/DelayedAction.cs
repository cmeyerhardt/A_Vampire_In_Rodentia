using UnityEngine;
using UnityEngine.Events;

public class DelayedAction : MonoBehaviour
{
    [SerializeField] float delayInSeconds = 2f;
    public UnityEvent timerFinished;

    private void Start()
    {
        Destroy(this, delayInSeconds + 2f);
    }

    void Update()
    {
        if(delayInSeconds > 0f)
        {
            delayInSeconds -= Time.deltaTime;
        }
        else
        {
            timerFinished.Invoke();
        }
    }


}
