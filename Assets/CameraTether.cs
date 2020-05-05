using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTether : MonoBehaviour
{
    //Cache
    Camera m_camera; //todo -- reference player camera if more than one camera

    private void Awake()
    {
        m_camera = Camera.main;
    }

    void Start()
    {
        
    }

    void Update()
    {
        //cast a ray to the camera
        Ray ray = new Ray(transform.position, (m_camera.transform.position - transform.position));

        RaycastHit[] hits = Physics.RaycastAll(ray);
        //sort raycast by distance
        float[] distances = new float[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            distances[i] = hits[i].distance;
        }
        Array.Sort(distances, hits);

        //check the first element of hits, 
        for(int i = 0; i < hits.Length; i++)
        {
            Transform hitTransform = hits[i].transform;

            // if  the ray hit a NON-TRIGGER collider, 
            if (hitTransform.GetComponent<Collider>() != null && !hitTransform.GetComponent<Collider>().isTrigger)
            {
                // Check which reference it is
                if (!ReferenceEquals(hitTransform.gameObject, m_camera.gameObject))
                {
                    // if its not the camera, change the camera's position to be where the first hit is
                    m_camera.transform.position = hitTransform.position;
                }
            }
        }
    }
}
