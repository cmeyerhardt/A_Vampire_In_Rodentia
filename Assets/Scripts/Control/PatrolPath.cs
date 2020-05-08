using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    [Header("Child objects are used as waypoints in a patrol path.")]
    [SerializeField] Color gizmosColor = Color.white;
    int i;

    private void OnDrawGizmos()
    {
        for (i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);

            Gizmos.color = gizmosColor;
            Gizmos.DrawSphere(GetWaypoint(i), .2f);
            Gizmos.DrawLine(GetWaypoint(i), GetWaypoint(j));
        }
    }

    public int GetCurrentIndex()
    {
        return i;
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

    public Vector3 GetWaypoint(int i)
    {
        if (i == transform.childCount)
        {
            i = 0;
        }
        return transform.GetChild(i).position;
    }

    public Vector3 GetNextWaypoint(int i)
    {
        if (i + 1 == transform.childCount)
        {
            i = 0;
        }
        return transform.GetChild(i).position;
    }
}
