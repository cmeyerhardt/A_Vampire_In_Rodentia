using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] [Range(0f, 360f)] float turnSpeed = 30f;
    Movement movement = null;

    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    void Update()
    {
        // move forward or back
        if (Input.GetKey(KeyCode.W))
        {
            movement.MoveTo(transform.forward);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement.MoveTo(-transform.forward);
        }

        // move left or right
        if (Input.GetKey(KeyCode.A))
        {
            movement.MoveTo(-transform.right);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            movement.MoveTo(transform.right);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.down * turnSpeed * Time.deltaTime);
            //transform.RotateAround(transform.position, Vector3.up, 45f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
            //transform.RotateAround(transform.position, Vector3.down, 45f);
        }
    }
}
