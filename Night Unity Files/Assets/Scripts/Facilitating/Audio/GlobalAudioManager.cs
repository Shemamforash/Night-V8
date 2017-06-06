using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class GlobalAudioManager : MonoBehaviour
{
    private List<AudioSource> musicSources = new List<AudioSource>();
    private List<AudioSource> effectsSources = new List<AudioSource>();
    public Slider masterSlider, musicSlider, effectsSlider;

    private void UpdateVolumes()
    {
        foreach (AudioSource a in musicSources)
        {
            if (Settings.masterVolume < Settings.musicVolume)
            {
                a.volume = Settings.masterVolume;
            }
            else
            {
                a.volume = Settings.musicVolume;
            }
        }
        foreach (AudioSource e in effectsSources)
        {
            if (Settings.masterVolume < Settings.effectsVolume)
            {
                e.volume = Settings.masterVolume;
            }
            else
            {
                e.volume = Settings.effectsVolume;
            }
        }
    }

    public void SetMasterVolume(Slider slider)
    {
        Settings.masterVolume = slider.value;
        UpdateVolumes();
    }

    public void SetEffectsVolume(Slider slider)
    {
        Settings.effectsVolume = slider.value;
        UpdateVolumes();
    }

    public void SetMusicVolume(Slider slider)
    {
        Settings.musicVolume = slider.value;
        UpdateVolumes();
    }

    public void Initialise()
    {
        List<GameObject> musicObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("MusicSource"));
        List<GameObject> effectsObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("SFXSource"));
        foreach (GameObject m in musicObjects)
        {
            musicSources.Add(m.GetComponent<AudioSource>());
        }
        foreach (GameObject e in effectsObjects)
        {
            effectsSources.Add(e.GetComponent<AudioSource>());
        }

        masterSlider.value = Settings.masterVolume;
        musicSlider.value = Settings.musicVolume;
        effectsSlider.value = Settings.effectsVolume;
        UpdateVolumes();
    }
}
