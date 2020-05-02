using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Zoom")]
    [SerializeField] float minCameraDist = 4f;
    [SerializeField] float maxCameraDist = 20f;
    [SerializeField] float currentCameraDist;

    Camera m_camera = null;

    private void Awake()
    {
        m_camera = Camera.main;
    }

    void LateUpdate()
    {
        if(Input.mouseScrollDelta.y > 0f)
        {
            float newZ = m_camera.transform.localPosition.z + 1f;
            if (Mathf.Abs(newZ) > minCameraDist)
            {
                m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, newZ);
                currentCameraDist = (transform.position - m_camera.transform.position).magnitude;
            }
        }

        if (Input.mouseScrollDelta.y < 0f)
        {
            float newZ = m_camera.transform.localPosition.z - 1f;
            if (Mathf.Abs(newZ) < maxCameraDist)
            {
                m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, newZ);
                currentCameraDist = (transform.position - m_camera.transform.position).magnitude;
            }
        }
    }
}
