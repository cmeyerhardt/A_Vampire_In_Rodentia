using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Options")]
    [SerializeField] float timeToWait = 0f;
    [SerializeField] bool randomWaitTime = false;
    [SerializeField] float randomWaitVariation = 2f;

    public float GetWaitTime()
    {
        float waitTime = timeToWait;
        if(randomWaitTime)
        {
            waitTime += UnityEngine.Random.Range(-randomWaitVariation, randomWaitVariation);
        }
        return waitTime;
    }
}
