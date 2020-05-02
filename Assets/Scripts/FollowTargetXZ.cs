using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetXZ : MonoBehaviour
{
    [SerializeField] Transform target = null;

    void LateUpdate()
    {
        if (target == null) { return; }

        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
    }
}
