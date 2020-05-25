using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ColorizeType { MeshRenderer, MeshRenderer_Emissive, SpriteRenderer, Image, Light, TextMeshProUGUI, ParticleSystem }

[System.Serializable]
public class ColorizeSet
{
    [SerializeField] public ColorizeType type;
    [SerializeField] public GameObject[] gameObject;
}

public class Colorizer : MonoBehaviour
{
    public bool recolorInStart = false;

    [Header("Configure")]
    [SerializeField] Color color = Color.white;
    [SerializeField] ColorizeSet[] colorizeSets = null;

    private void Start()
    {
        if (recolorInStart)
        {
            Recolor();
        }
    }

    private void Recolor()
    {
        Recolor(color);
    }

    public void Recolor(Color newColor)
    {
        if (colorizeSets.Length <= 0) { return; }

        Color _color = newColor;

        foreach (ColorizeSet colorSet in colorizeSets)
        {
            foreach (GameObject objectToColor in colorSet.gameObject)
            {
                switch (colorSet.type)
                {
                    case ColorizeType.Image:
                        objectToColor.GetComponent<Image>().color = _color;
                        break;
                    case ColorizeType.MeshRenderer:
                        objectToColor.GetComponent<MeshRenderer>().material.SetColor("_Color", _color);
                        break;
                    case ColorizeType.MeshRenderer_Emissive:
                        objectToColor.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", _color);
                        break;
                    case ColorizeType.SpriteRenderer:
                        objectToColor.GetComponent<SpriteRenderer>().color = _color;
                        break;
                    case ColorizeType.Light:
                        objectToColor.GetComponent<Light>().color = _color;
                        break;
                    case ColorizeType.TextMeshProUGUI:
                        objectToColor.GetComponent<TextMeshProUGUI>().color = _color;
                        break;
                    case ColorizeType.ParticleSystem:
                        StartCoroutine(ColorParticles(_color, objectToColor.GetComponent<ParticleSystem>()));
                        break;
                }
            }
        }
    }

    public IEnumerator ColorParticles(Color _color, params ParticleSystem[] particles)
    {
        if (particles != null)
        {
            foreach (ParticleSystem particle in particles)
            {
                var main = particle.main;
                main.startColor = _color;
            }
        }
        yield return null;
    }
}
