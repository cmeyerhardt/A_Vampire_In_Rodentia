using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GizmoType { Sphere, WireSphere, Both }
public class GizmoSphere : MonoBehaviour
{
    [Header("Draw Gizmos")]
    [SerializeField] public GizmoType gizmoType = GizmoType.Sphere;
    [SerializeField] public Color gizmoColor = Color.white;
    [SerializeField] [Range(0.1f, 50f)] public float radius = .5f;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        if(gizmoType == GizmoType.Sphere)
        {
            Gizmos.DrawSphere(transform.position, radius);
        }
        else if (gizmoType == GizmoType.WireSphere)
        {
            Gizmos.DrawWireSphere(transform.position, radius);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
