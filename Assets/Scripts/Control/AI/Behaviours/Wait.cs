using UnityEngine;

public class Wait : AIBehaviour
{
    [Header("Wait--")]
    [SerializeField] public float duration = 3f;
    [SerializeField] public bool waitIndefinitly = false;
    [SerializeField] [Range(0f, 10f)] float movementFraction = .75f;
    public float timer = 0f;

    public new void OnEnable()
    {
        ai.MoveToDestination(transform.position, movementFraction);

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
