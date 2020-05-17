using UnityEngine;

public class Wait : AIBehaviour
{
    [SerializeField] public float duration = 3f;
    [SerializeField] public bool waitIndefinitly = false;

    public float timer = 0f;

    public new void OnEnable()
    {
        ai.MoveToDestination(transform.position, .75f);

        if (!waitIndefinitly)
        {
            timer = duration;
        }
    }

    new void Update()
    {
        if (waitIndefinitly)
        {
            //do nothing
        }
        else
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                doneEvent.Invoke(this);
            }
        }
    }
}
