using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoomDistance : MonoBehaviour
{
    [Header("Zoom camera by scrolling mouse wheel")]
    [SerializeField] float minCameraDist = 4f;
    [SerializeField] float maxCameraDist = 10f;
    [Header("Will the camera correct on collider intersection?")]
    public bool correctCameraTether = true;
    [SerializeField] [Range(.1f, 50f)]float correctionSpeed = 10f;

    // Cache 
    float zoomDistance = 0f;
    float currentZoom = 0f;
    float newZ;
    
    Ray ray;
    Camera m_camera = null;

    private void Awake()
    {
        m_camera = Camera.main;
    }

    private void Start()
    {
        zoomDistance = (m_camera.transform.position - transform.position).magnitude;
        newZ = m_camera.transform.localPosition.z;
    }

    void Update()
    {
        if (Input.mouseScrollDelta.y != 0f)
        {
            // Calculate the new distance
            newZ = Mathf.Clamp(newZ - Input.mouseScrollDelta.y, minCameraDist, maxCameraDist);

            // Set the new position
            m_camera.transform.localPosition = new Vector3(m_camera.transform.localPosition.x,
                                                           m_camera.transform.localPosition.y,
                                                           -newZ);
            // Record the desired zoom distance
            zoomDistance = (m_camera.transform.position - transform.position).magnitude;
        }

        currentZoom = (m_camera.transform.position - transform.position).magnitude;
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
        if(correctCameraTether)
        {
            //cast a ray toward the camera
            ray = new Ray(transform.position, (m_camera.transform.position - transform.position).normalized);

            RaycastHit[] hits = Physics.RaycastAll(ray, zoomDistance);

            //Sort hits by distance and change the camer's position so nothing obstructive blocks the camera's view of the player
            if (hits.Length > 0)
            {
                // Sory hits by distance
                List<RaycastHit> hitList = SetHitList(hits);

                if (hitList.Count > 0)
                {
                    // If the closest object the raycast hit is the camera
                    if (hitList[0].transform.gameObject == m_camera.gameObject)
                    {
                        // If the camera's distance is not at the desired zoom distance
                        if (currentZoom < zoomDistance)
                        {
                            // if camera is the only thing hit
                            if (hitList.Count == 1)
                            {
                                // move camera back to zoomDistance
                                m_camera.transform.localPosition =
                                    Vector3.Lerp(m_camera.transform.localPosition,
                                        new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, -zoomDistance),
                                        correctionSpeed * Time.deltaTime);
                            }
                            // or if the camera is closer than the next thing hit
                            else if (hitList[0].distance < hitList[1].distance - 1f)
                            {
                                //todo - move camera back to the next closest object
                                m_camera.transform.localPosition =
                                    Vector3.Lerp(m_camera.transform.localPosition,
                                        new Vector3(m_camera.transform.localPosition.x, m_camera.transform.localPosition.y, -hitList[1].distance),
                                        correctionSpeed * Time.deltaTime);
                            }
                        }
                    }
                    else
                    {
                        // change the camera's position to that of the closest hit
                        if (hitList[0].transform.gameObject != m_camera.gameObject)
                        {
                            //todo -- camera clips with floor tiles
                            m_camera.transform.position = Vector3.Lerp(m_camera.transform.position, hitList[0].point, 2f * correctionSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }
    }


    // Debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ray);
    }

    public string PrintRaycastHits(RaycastHit[] hits)
    {
        string s = "";
        foreach (RaycastHit hit in hits)
        {
            s += hit.transform.gameObject.name + ", ";
        }
        return s;
    }
}