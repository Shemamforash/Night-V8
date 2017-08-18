using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Facilitating.Persistence;
using SamsHelper.Persistence;

namespace Audio
{
    using Persistence;
    public class GlobalAudioManager : MonoBehaviour
    {
        private List<AudioSource> musicSources = new List<AudioSource>();
        private List<AudioSource> effectsSources = new List<AudioSource>();
        public Slider masterSlider, musicSlider, effectsSlider;
        private float musicVolume, effectsVolume, masterVolume;
        private PersistenceListener persistenceListener;

        public void Awake(){
            persistenceListener = new PersistenceListener(Load, Save, "Audio Manager");
        }

        private void Load(){
            musicVolume = GameData.MusicVolume;
            effectsVolume = GameData.EffectsVolume;
            masterVolume = GameData.MasterVolume;
        }

        private void Save(){
            GameData.MusicVolume = musicVolume;
            GameData.EffectsVolume = effectsVolume;
            GameData.MasterVolume = masterVolume;
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

            masterSlider.value = masterVolume;
            musicSlider.value = musicVolume;
            effectsSlider.value = effectsVolume;
            UpdateVolumes();
        }
    }
}