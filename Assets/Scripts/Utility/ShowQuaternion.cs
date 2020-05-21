using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShowQuaternion : MonoBehaviour
{
    public Quaternion rotation;
    public bool setQuaternion = false;


    void Update()
    {
        if(setQuaternion)
        {
            transform.rotation = rotation;
        }
        else
        {
            rotation = transform.rotation;
        }
    }
}
