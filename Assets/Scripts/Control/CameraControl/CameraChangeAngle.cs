using UnityEngine;

public class CameraChangeAngle : MonoBehaviour
{
    [Header("Rotate an object around its pivot via mouse input")]

    [SerializeField] [Range(750f, 2000f)] float panSpeed = 1000f;
    public bool allowXRotation = true;
    public bool allowYRotation = true;

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float x = 0f;
            float y = 0f;

            if(allowXRotation)
            {
                x = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;
            }
            if(allowYRotation)
            {
                y = Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            }

            // Rotate the camera with respect to mouse movement
            transform.Rotate(new Vector3(x, y, 0f));

            x = transform.rotation.eulerAngles.x;
            y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(x, y, 0);
        }
    }
}


