using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Facilitating.Audio
{
    public class GlobalAudioManager : MonoBehaviour, IPersistenceTemplate
    {
        private static float _volume;
        public Slider MasterSlider;
        private static AudioMixer _masterMixer;

        public void Load(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Settings) return;
            XmlNode node = root.GetNode("SoundSettings");
            _volume = node.FloatFromNode(nameof(_volume));
            Initialise();
        }

        public XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Settings) return null;
            XmlNode node = SaveController.CreateNodeAndAppend("SoundSettings", root);
            SaveController.CreateNodeAndAppend(nameof(_volume), node, _volume);
            return node;
        }

        public static float Volume()
        {
            return _volume;
        }

        public void Awake()
        {
            SaveController.AddPersistenceListener(this);
        }

        private static float NormalisedVolumeToAttentuation(float volume)
        {
            return 1 - Mathf.Sqrt(volume) * -80f;
        }

        public static void SetVolume(float volume)
        {
            _volume = volume;
            if(_masterMixer == null) _masterMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _masterMixer.SetFloat("Master", NormalisedVolumeToAttentuation(_volume));
        }

        private void Initialise()
        {
            MasterSlider.onValueChanged.AddListener(SetVolume);
            MasterSlider.value = _volume;
            SetVolume(_volume);
        }
    }
}