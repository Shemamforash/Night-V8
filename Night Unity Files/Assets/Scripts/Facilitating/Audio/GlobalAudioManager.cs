﻿using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.Audio
{
    public class GlobalAudioManager : MonoBehaviour, IPersistenceTemplate
    {
        private readonly List<AudioSource> _effectsSources = new List<AudioSource>();
        private readonly List<AudioSource> _musicSources = new List<AudioSource>();
        private float _musicVolume, _effectsVolume, _masterVolume;
        public Slider MasterSlider, MusicSlider, EffectsSlider;

        public void Load(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Settings) return;
            XmlNode node = root.GetNode("SoundSettings");
            _musicVolume = node.FloatFromNode(nameof(_musicVolume));
            _effectsVolume = node.FloatFromNode(nameof(_effectsVolume));
            _masterVolume = node.FloatFromNode(nameof(_masterVolume));
            Initialise();
        }

        public XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Settings) return null;
            XmlNode node = SaveController.CreateNodeAndAppend("SoundSettings", root);
            SaveController.CreateNodeAndAppend(nameof(_musicVolume), node, _musicVolume);
            SaveController.CreateNodeAndAppend(nameof(_effectsVolume), node, _effectsVolume);
            SaveController.CreateNodeAndAppend(nameof(_masterVolume), node, _masterVolume);
            return node;
        }

        public void Awake()
        {
            SaveController.AddPersistenceListener(this);
        }

        private void UpdateVolumes()
        {
            foreach (AudioSource a in _musicSources) a.volume = _masterVolume < _musicVolume ? _masterVolume : _musicVolume;
            foreach (AudioSource e in _effectsSources) e.volume = _masterVolume < _effectsVolume ? _masterVolume : _effectsVolume;
        }

        private void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
            UpdateVolumes();
        }

        private void SetEffectsVolume(float volume)
        {
            _effectsVolume = volume;
            UpdateVolumes();
        }

        private void SetMusicVolume(float volume)
        {
            _musicVolume = volume;
            UpdateVolumes();
        }

        private void Initialise()
        {
            List<GameObject> musicObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("MusicSource"));
            List<GameObject> effectsObjects = new List<GameObject>(GameObject.FindGameObjectsWithTag("SFXSource"));
            foreach (GameObject m in musicObjects) _musicSources.Add(m.GetComponent<AudioSource>());
            foreach (GameObject e in effectsObjects) _effectsSources.Add(e.GetComponent<AudioSource>());

            MasterSlider.onValueChanged.AddListener(SetMasterVolume);
            MusicSlider.onValueChanged.AddListener(SetMusicVolume);
            EffectsSlider.onValueChanged.AddListener(SetEffectsVolume);

            MasterSlider.value = _masterVolume;
            MusicSlider.value = _musicVolume;
            EffectsSlider.value = _effectsVolume;
            UpdateVolumes();
        }
    }
}