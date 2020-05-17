using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderRegion : GizmoSphere
{
    public List<GameObject> interactables = new List<GameObject>();


    void Start()
    {
        SphereCollider c = GetComponent<SphereCollider>();
        if (c != null)
        {
            c.radius = radius;
        }
    }

    public GameObject GetRandomInteractible(GameObject requester)
    {
        if (interactables.Count > 1)
        {
            int i = Random.Range(0, interactables.Count);

            return interactables[i] == requester
                ? (i == 0 ? interactables[i + 1] : interactables[i - 1])
                : interactables[i];
        }
        else
        {
            return null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Character>() != null && other.gameObject.tag != "Player")
        {
            if (!interactables.Contains(other.gameObject))
            {
                interactables.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(interactables.Contains(other.gameObject))
        {
            interactables.Remove(other.gameObject);
        }
    }
}
