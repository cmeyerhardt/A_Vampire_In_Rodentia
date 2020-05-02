using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] Transform target = null;

    void LateUpdate()
    {
        if (target == null) { return; }

        transform.LookAt(target);
    }
}
