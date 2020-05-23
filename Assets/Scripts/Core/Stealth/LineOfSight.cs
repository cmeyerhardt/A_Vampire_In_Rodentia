using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//LineOfSight
//Use to detect player
// can be used for: alarms, illumination
public class LineOfSight : MonoBehaviour
{
    [HideInInspector] public PlayerController player;
    [HideInInspector] public float distance;
    public Transform fromTransform;
    //Debug
    Ray ray;

    public virtual void Awake()
    {
        fromTransform = transform;
        player = FindObjectOfType<PlayerController>();
    }

    public bool RaycastToPlayerSuccessful( out GameObject firstHit)
    {
        firstHit = null;
        //cast a ray toward the player
        ray = new Ray(fromTransform.position, (player.transform.position - fromTransform.position).normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance + 2f);

        if (hits.Length > 0)
        {
            // Sory hits by distance
            List<RaycastHit> hitList = SetHitList(hits);

            if (hitList.Count > 0)
            {
                firstHit = hitList[0].transform.gameObject;
                if (ReferenceEquals(hitList[0].transform.gameObject,player.gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public bool RaycastToPlayerSuccessful()
    {
        //cast a ray toward the player
        ray = new Ray(fromTransform.position, (player.transform.position - fromTransform.position).normalized);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance + 2f);

        if (hits.Length > 0)
        {
            // Sory hits by distance
            List<RaycastHit> hitList = SetHitList(hits);

            if (hitList.Count > 0)
            {
                if (ReferenceEquals(hitList[0].transform.gameObject, player.gameObject))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private List<RaycastHit> SetHitList(RaycastHit[] hits)
    {
        List<RaycastHit> outList = new List<RaycastHit>();

        foreach (RaycastHit hit in hits)
        {
            bool added = false;
            if (hit.transform.gameObject != null && !hit.collider.isTrigger && hit.transform.gameObject != gameObject)
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

                    //print(outList[0].transform.name);
                    if (!added)
                    {
                        outList.Add(hit);
                    }
                }
            }
        }
        return outList;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(ray);
    }
}
