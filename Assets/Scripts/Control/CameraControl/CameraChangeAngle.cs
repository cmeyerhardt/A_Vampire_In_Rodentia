using UnityEngine;

public class CameraChangeAngle : MonoBehaviour
{
    [Header("Rotate an object around its pivot via mouse input")]
    [SerializeField] [Tooltip("0: Right-Click, 1: Left-Click, 2: Middle-Click")][Range(0, 2)] int mouseButton = 1;
    [SerializeField] [Range(750f, 2000f)] float panSpeed = 1000f;
    public bool takeXInput = true;
    public bool takeYInput = true;

    void Update()
    {
        if (Input.GetMouseButton(mouseButton))
        {
            float x = 0f;
            float y = 0f;

            if(takeXInput)
            {
                x = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;
            }
            if(takeYInput)
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


