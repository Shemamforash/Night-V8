using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class GlobalAudioManager : MonoBehaviour
{
    // Volume values static to prevent reset between scenes
    private static float masterVolume = 1, effectsVolume = 1, musicVolume = 1;
    private List<AudioSource> musicSources = new List<AudioSource>();
    private List<AudioSource> effectsSources = new List<AudioSource>();

    public void Start()
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
    }

    private void UpdateVolumes()
    {
        foreach (AudioSource a in musicSources)
        {
            if (masterVolume < musicVolume)
            {
                a.volume = masterVolume;
            }
            else
            {
                a.volume = musicVolume;
            }
        }
        foreach (AudioSource e in effectsSources)
        {
            if (masterVolume < effectsVolume)
            {
				e.volume = masterVolume;
            }
            else
            {
                e.volume = effectsVolume;
            }
        }
    }

    public void SetMasterVolume(Slider slider)
    {
        masterVolume = slider.value;
        UpdateVolumes();
    }

    public void SetEffectsVolume(Slider slider)
    {
        effectsVolume = slider.value;
        UpdateVolumes();
    }

    public void SetMusicVolume(Slider slider)
    {
        musicVolume = slider.value;
        UpdateVolumes();
    }
}
