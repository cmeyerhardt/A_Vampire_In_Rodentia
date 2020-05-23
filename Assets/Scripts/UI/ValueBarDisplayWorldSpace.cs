using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueBarDisplayWorldSpace : ValueBarDisplay
{
    [SerializeField] SpriteRenderer bar = null;
    [SerializeField] GameObject showHide = null;
    public bool hideWhenFull = true;
    public bool hideWhenEmpty = true;


    public override void UpdateValue(float percentage)
    {
        if ((percentage == 0f && hideWhenEmpty) || (percentage == 1f && hideWhenFull))
        {
            Show(false);
        }
        else
        {
            Show(true);
        }
        base.UpdateValue(percentage);
    }
    
    private void Show(bool show)
    {
        showHide.SetActive(show);
    }

    public void ChangeColor(Color color)
    {
        bar.color = color;
    }
}
