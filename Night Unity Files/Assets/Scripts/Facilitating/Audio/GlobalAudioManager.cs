using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Persistence;
using SamsHelper.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.Audio
{
    public class GlobalAudioManager : MonoBehaviour, IPersistenceTemplate
    {
        private readonly List<AudioSource> _musicSources = new List<AudioSource>();
        private readonly List<AudioSource> _effectsSources = new List<AudioSource>();
        public Slider MasterSlider, MusicSlider, EffectsSlider;
        private float _musicVolume, _effectsVolume, _masterVolume;

        public void Awake()
        {
            SaveController.AddPersistenceListener(this);
        }
        
        public void Load(XmlNode root, PersistenceType saveType)
        {
            if (saveType == PersistenceType.Settings)
            {
                XmlNode node = root.SelectSingleNode("SoundSettings");
                _musicVolume = SaveController.ParseFloatFromNodeAndString(node, nameof(_musicVolume));
                _effectsVolume = SaveController.ParseFloatFromNodeAndString(node, nameof(_musicVolume));
                _masterVolume = SaveController.ParseFloatFromNodeAndString(node, nameof(_musicVolume));
                Initialise();
            }
        }

        public void Save(XmlNode root, PersistenceType saveType)
        {
            if (saveType == PersistenceType.Settings)
            {
                XmlNode node = SaveController.CreateNodeAndAppend("SoundSettings", root);
                SaveController.CreateNodeAndAppend(nameof(_musicVolume), node, _musicVolume);
                SaveController.CreateNodeAndAppend(nameof(_effectsVolume), node, _effectsVolume);
                SaveController.CreateNodeAndAppend(nameof(_masterVolume), node, _masterVolume);
            }
        }

        private void UpdateVolumes()
        {
            foreach (AudioSource a in _musicSources)
            {
                a.volume = _masterVolume < _musicVolume ? _masterVolume : _musicVolume;
            }
            foreach (AudioSource e in _effectsSources)
            {
                e.volume = _masterVolume < _effectsVolume ? _masterVolume : _effectsVolume;
            }
        }

        public void SetMasterVolume(Slider slider)
        {
            _masterVolume = slider.value;
            UpdateVolumes();
        }

        public void SetEffectsVolume(Slider slider)
        {
            _effectsVolume = slider.value;
            UpdateVolumes();
        }

        public void SetMusicVolume(Slider slider)
        {
            _musicVolume = slider.value;
            UpdateVolumes();
        }

        private void Initialise()
        {
            List<GameObject> musicObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("MusicSource"));
            List<GameObject> effectsObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("SFXSource"));
            foreach (GameObject m in musicObjects)
            {
                _musicSources.Add(m.GetComponent<AudioSource>());
            }
            foreach (GameObject e in effectsObjects)
            {
                _effectsSources.Add(e.GetComponent<AudioSource>());
            }

            MasterSlider.value = _masterVolume;
            MusicSlider.value = _musicVolume;
            EffectsSlider.value = _effectsVolume;
            UpdateVolumes();
        }
    }
}