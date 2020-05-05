using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraZoomDistance : MonoBehaviour
{
    [Header("Change camera distance by scrolling mouse wheel")]
    [SerializeField] float minCameraDist = 4f;
    [SerializeField] float maxCameraDist = 10f;
    [SerializeField] [Range(.1f, 1f)]float cameraIncrement = .1f;
    Ray ray;

    Camera m_camera = null;
    Transform player = null;

    public float zoomDistance = 0f;
    public float currentZoom = 0f;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        m_camera = Camera.main;
    }

    private void Start()
    {
        zoomDistance = (m_camera.transform.position - transform.position).magnitude;
    }

    void Update()
    {
        
        if (Input.mouseScrollDelta.y > 0f) //zoom in
        {
            float newZ = m_camera.transform.localPosition.z + cameraIncrement;

            if (Mathf.Abs(newZ) > minCameraDist)
            {
                m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, newZ);
                zoomDistance = (m_camera.transform.position - transform.position).magnitude;
            }
        }

        else if (Input.mouseScrollDelta.y < 0f) //zoom out
        {
            float newZ = m_camera.transform.localPosition.z - cameraIncrement;
            if (Mathf.Abs(newZ) < maxCameraDist)
            {
                m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, newZ);
                zoomDistance = (m_camera.transform.position - transform.position).magnitude;
            }
        }
        //m_camera.transform.LookAt(transform.position);

        currentZoom = (m_camera.transform.position - transform.position).magnitude;
    }

    public string ToString(float[] a)
    {
        string s = "";

        foreach (float b in a)
        {
            s += b + ", ";
        }
        return s;
    }

    public string ToString(RaycastHit[] hits)
    {
        string s = "";
        foreach(RaycastHit hit in hits)
        {
            s += hit.transform.gameObject.name + ", ";
        }
        return s;
    }

    private List<RaycastHit> SetHitList(RaycastHit[] hits)
    {
        List<RaycastHit> outList = new List<RaycastHit>();

        foreach(RaycastHit hit in hits)
        {
            bool added = false;
            if (hit.transform.gameObject != null && !hit.collider.isTrigger)
            {
                if (outList.Count == 0)
                {
                    outList.Add(hit);
                }
                else
                {
                    // insert into list IN CORRECT POSITION
                    for (int i = 0; i < outList.Count; i++)
                    {
                        if (outList[i].distance > hit.distance)
                        {
                            outList.Insert(i, hit);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        outList.Add(hit);
                    }
                }
            }
        }
        return outList;
    }

    void LateUpdate()
    {
        //cast a ray toward the camera
        ray = new Ray(transform.position, (m_camera.transform.position - transform.position).normalized);

        RaycastHit[] hits = Physics.RaycastAll(ray, zoomDistance);

        if (hits.Length > 0)
        {
            // Sory hits by distance
            List<RaycastHit> hitList = SetHitList(hits);

            if (hitList.Count > 0)
            {
                // If the closest object the raycast hit is the camera
                //if (hits[0].transform.gameObject == m_camera.gameObject)
                if (hitList[0].transform.gameObject == m_camera.gameObject)
                {
                    // If the camera's distance is not at the desired zoom distance
                    if (currentZoom < zoomDistance)
                    {
                        // AND either ONLY the camera was hit by the ray, or the camera is currently closer than the NEXT object hit
                        if (hitList.Count == 1)
                        {
                            Debug.Log("Move camera back to zoomDistance");
                            m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, -zoomDistance);
                        }
                        //else if (distances[0] < (distances[1] - 1f))
                        else if (hitList[0].distance < hitList[1].distance - 1f)
                        {
                            Debug.Log("Move camera back to next closest object");
                            m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, -hitList[1].distance);
                        }
                    }
                }
                else
                {
                    if (hitList[0].transform.gameObject != m_camera.gameObject)
                    {
                        m_camera.transform.position = hitList[0].point;
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(ray);
    }
}