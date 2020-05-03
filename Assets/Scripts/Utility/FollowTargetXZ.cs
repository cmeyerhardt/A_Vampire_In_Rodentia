using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetXZ : MonoBehaviour
{
    [Header("This object's transform will match the target's x- and z-position")]
    [SerializeField] Transform target = null;

    void LateUpdate()
    {
        if (target == null) { return; }

        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
    }
}
