using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer _MasterMixer;
    AudioMixerSnapshot masterVolume;

    private void Awake()
    {
        
    }

    public void SetVolume(float value)
    {
        _MasterMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20f);
    }
}
