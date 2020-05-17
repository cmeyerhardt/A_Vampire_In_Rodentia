using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour
{
    private void Awake()
    {
        Singleton[] others = FindObjectsOfType<Singleton>();
        if (others.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
