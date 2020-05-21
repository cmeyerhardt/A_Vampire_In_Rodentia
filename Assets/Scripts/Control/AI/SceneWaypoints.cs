using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneWaypoints : MonoBehaviour
{
    //Dictionary<string, GameObject> waypoints = new Dictionary<string, GameObject>();

    public Transform[] GetWaypoints(string waypointsName)
    {
        Transform[] waypoints = transform.Find(waypointsName).GetComponentsInChildren<Transform>();

        return waypoints;
    }

    
}
