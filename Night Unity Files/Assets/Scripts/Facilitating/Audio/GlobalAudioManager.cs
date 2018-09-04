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
        private static float _masterVolume = 1, _modifiedVolume;
        private static AudioMixer _audioMixer;

        public void Load(XmlNode root)
        {
            XmlNode node = root.GetNode("SoundSettings");
            _masterVolume = node.FloatFromNode(nameof(_masterVolume));
        }

        public XmlNode Save(XmlNode root)
        {
            XmlNode node = root.CreateChild("SoundSettings");
            node.CreateChild(nameof(_masterVolume), _masterVolume);
            return node;
        }

        public static float Volume()
        {
            return _masterVolume;
        }

        private static float NormalisedVolumeToAttenuation(float volume)
        {
            if (volume == 0) volume = 0.001f;
            return Mathf.Log(volume) * 20f;
        }

        public static void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.SetFloat("Master", NormalisedVolumeToAttenuation(_masterVolume));
        }

        public static void SetModifiedVolume(float volume)
        {
            _modifiedVolume = volume;
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.SetFloat("Modified", NormalisedVolumeToAttenuation(_modifiedVolume));
        }
    }
}