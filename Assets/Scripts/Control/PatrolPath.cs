using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    [Header("Child objects are used as waypoints in a patrol path.")]
    [SerializeField] Color gizmosColor = Color.white;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);

            Gizmos.color = gizmosColor;
            Gizmos.DrawSphere(GetWaypointPosition(i), .2f);
            Gizmos.DrawLine(GetWaypointPosition(i), GetWaypointPosition(j));
        }
    }

    public int GetNextIndex(int i)
    {
        if (i + 1 == transform.childCount)
        {
            return 0;
        }
        return i + 1;
    }
    
    public int GetRandomIndex()
    {
        return Random.Range(0, transform.childCount - 1);
    }

    public Vector3 GetWaypointPosition(int i)
    {
        if (i == transform.childCount)
        {
            i = 0;
        }
        return transform.GetChild(i).position;
    }

    public Vector3 GetNextWaypointPosition(int i)
    {
        if (i + 1 == transform.childCount)
        {
            i = 0;
        }
        return transform.GetChild(i).position;
    }

    public Transform GetWaypointTransform(int i)
    {
        return transform.GetChild(i);
    }

    public Waypoint GetWaypoint(int i)
    {
        return transform.GetChild(i).GetComponent<Waypoint>();
    }
}
