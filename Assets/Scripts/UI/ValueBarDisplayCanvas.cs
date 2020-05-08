using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValueBarDisplayCanvas : ValueBarDisplay
{
    [SerializeField] Image bar = null;

    public override void UpdateValue(float percentage)
    {
        bar.fillAmount = Mathf.Clamp(percentage, 0f, 1f);
    }

    public void ChangeColor(Color color)
    {
        bar.color = color;
    }
}
