using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text = null;

    public void GenerateText(string message, Color? color = null)
    {
        if (color != null)
        {
            text.color = (Color)color;
        }
        else
        {
            text.color = Color.white;
        }
        text.text = message;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
