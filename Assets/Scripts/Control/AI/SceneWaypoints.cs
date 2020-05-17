using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneWaypoints : MonoBehaviour
{
    public Transform[] GetWaypoints(string waypointsName)
    {
        Transform[] waypoints = transform.Find(waypointsName).GetComponentsInChildren<Transform>();
        int i = 0;
        foreach(Transform waypoint in waypoints)
        {
            if(waypoint != null)
            {
                waypoints[i] = waypoint;
                i++;
            }
        }
        return waypoints;
    }
}
