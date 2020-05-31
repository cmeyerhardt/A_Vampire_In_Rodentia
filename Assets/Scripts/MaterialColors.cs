using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialColors : MonoBehaviour
{
    [SerializeField] SkinnedMeshRenderer skinnedMeshRend = null;

    [Header("Fur Color")]
    [SerializeField] Gradient furColorGradient = new Gradient();
    [SerializeField] bool useGradientToRandomizeFur = true;

    [SerializeField] Color[] furColors = null;
    [SerializeField] bool chooseRandomColorFromCollection = false;

    Material furMaterial = null;

    [Header("Shirt Color")]
    [SerializeField] Gradient shirtColorGradient = new Gradient();
    [SerializeField] bool useGradientToRandomizeShirt = true;

    [SerializeField] Color shirtColor = new Color();
    [SerializeField] bool randomizeMouseShirtColor = true;
    [SerializeField] bool randomizeRed = true;
    [SerializeField] bool randomizeGreen = true;
    [SerializeField] bool randomizeBlue = true;
    Material shirtMaterial = null;

    Health health = null;

    public void Awake()
    {
        health = GetComponent<Health>();

        Material[] materials = skinnedMeshRend.materials;
        if (materials != null && materials.Length > 0)
        {
            furMaterial = materials[0];
            if (furMaterial != null && furColors != null && furColors.Length > 0)
            {
                if (useGradientToRandomizeFur)
                {
                    furMaterial.SetColor("_Color", furColorGradient.Evaluate(Random.Range(0f, 1f)));
                }
                else if (chooseRandomColorFromCollection)
                {
                    furMaterial.SetColor("_Color", furColors[Random.Range(0, furColors.Length - 1)]);
                }
                else
                {
                    furMaterial.SetColor("_Color", furColors[0]);
                }
            }

            if(materials.Length > 1 && materials[1] != null)
            {
                shirtMaterial = materials[1];
                if (shirtMaterial != null)
                {
                    if (useGradientToRandomizeShirt)
                    {
                        shirtMaterial.SetColor("_Color", shirtColorGradient.Evaluate(Random.Range(0f, 1f)));
                    }
                    else if (randomizeMouseShirtColor || shirtColor.a == 0f)
                    {
                        Color randomShirtColor = new Color(randomizeRed ? Random.Range(0f, 1f) : 0f, randomizeGreen ? Random.Range(0f, 1f) : 0f, randomizeBlue ? Random.Range(0f, 1f) : 0f);
                        shirtMaterial.SetColor("_Color", randomShirtColor);
                    }
                    else if (!randomizeMouseShirtColor && shirtColor.a > 0f)
                    {
                        shirtMaterial.SetColor("_Color", shirtColor);
                    }
                }
            }

        }
    }

    public void ChangeFurColor(float _ph)
    {
        if (health != null && furMaterial != null)
        {
            furMaterial.SetColor("_Color", furMaterial.color + (Color.white - furMaterial.color) * (1 - health.GetHealthPerc()));
        }
    }
}
