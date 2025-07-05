using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start() {
       if (audioMixer == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("AudioSettingsUI: Missing references in the Inspector.");
            return;
        }

        // Set default volumes to 0 dB (full volume)
        audioMixer.SetFloat("MusicVolume", 0f);
        audioMixer.SetFloat("SFXVolume", 0f);

        // Set slider values to match 0 dB
        musicSlider.value = dBToSliderValue(0f); // Should be 100
        sfxSlider.value = dBToSliderValue(0f);   // Should be 100

        // Now add listeners
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        SetMusicVolume(100f);
        SetSFXVolume(100f);
    }

    public void SetMusicVolume(float sliderValue)
    {
        audioMixer.SetFloat("MusicVolume", sliderValueToDb(sliderValue));
    }

    public void SetSFXVolume(float sliderValue)
    {
        audioMixer.SetFloat("SFXVolume", sliderValueToDb(sliderValue));
    }

    // Converts 0–100 slider to dB (-80 to 0)
    private float sliderValueToDb(float sliderValue)
    {
        if (sliderValue <= 0.01f)
            return -80f; // effectively mute
        return Mathf.Log10(sliderValue / 100f) * 20f;
    }

    // Converts dB (-80 to 0) back to 0–100 slider
    private float dBToSliderValue(float dB)
    {
        return Mathf.Clamp(Mathf.Pow(10f, dB / 20f) * 100f, 0f, 100f);
    }
}
