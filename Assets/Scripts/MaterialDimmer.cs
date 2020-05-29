using System.Collections.Generic;
using UnityEngine;

public class MaterialDimmer : MonoBehaviour
{
    SkinnedMeshRenderer skinnedMeshRend = null;
    Material[] materials = null;

    //bool dimColor = false;
    //public bool fading = false;
    //float lowest = .4f;
    //float time = 3f;
    //float fadingTime = 3f;
    //Color originalColor = new Color();
    List<Color> originalColors = new List<Color>();
    Color dimmedColor = new Color(.5f, .5f, .5f, .5f);

    void Awake()
    {
        skinnedMeshRend = GetComponentInChildren<SkinnedMeshRenderer>();
        materials = skinnedMeshRend.materials;
        foreach(Material material in materials)
        {
            originalColors.Add(material.color);
        }
    }

    //void Update()
    //{
    //    if(fading)
    //    {
    //        fadingTime += Time.deltaTime;
    //        if (dimColor)
    //        {
    //            for (int i = 0; i < materials.Length; i++)
    //            {
    //                Material material = materials[i];
    //                if (material != null)
    //                {
    //                    Color color = Color.Lerp(
    //                        originalColors[i],
    //                        new Color(material.color.r * lowest, material.color.g * lowest, material.color.b * lowest, material.color.a * lowest),
    //                        time * Time.deltaTime);
    //                    material.SetColor("_Color", color);
    //                }
    //                if (fadingTime > time)
    //                {
    //                    material.SetColor("_Color", new Color(material.color.r * lowest, material.color.g * lowest, material.color.b * lowest, material.color.a * lowest));
    //                    fading = false;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            for (int i = 0; i < materials.Length; i++)
    //            {
    //                Material material = materials[i];
    //                if (material != null)
    //                {
    //                    Color color = Color.Lerp(
    //                        new Color(material.color.r * lowest, material.color.g * lowest, material.color.b * lowest, material.color.a * lowest),
    //                        originalColors[i],
    //                        Time.deltaTime/time);
    //                    material.SetColor("_Color", color);
    //                }
    //                if(fadingTime > time)
    //                {
    //                    print("original color done");
    //                    material.SetColor("_Color", originalColors[i]);
    //                    fading = false;
    //                }
    //            }
    //        }
    //    }

    //}
    public void RestoreMaterialColor()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material != null)
            {
                //print("Setting original material color");
                material.SetColor("_Color", originalColors[i]);
            }
        }
        //dimColor = true;
        //fadingTime = 0f;
        //fading = true;
    }
    public void DimMaterialColor()
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material != null)
            {
                //print("Setting dim material color");
                material.SetColor("_Color", dimmedColor);
            }
        }

        //dimColor = false;

        //fadingTime = 0f;
        //fading = true;
    }
}


