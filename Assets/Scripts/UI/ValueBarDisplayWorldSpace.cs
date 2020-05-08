using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBarDisplayWorldSpace : ValueBarDisplay
{
    [SerializeField] SpriteRenderer bar = null;

    public void ChangeColor(Color color)
    {
        bar.color = color;
    }
}
