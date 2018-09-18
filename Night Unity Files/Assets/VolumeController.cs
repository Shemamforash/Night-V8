using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    private Slider _volumeSlider;
    private static float _masterVolume = 1, _modifiedVolume;
    private static AudioMixer _audioMixer;

    private void Awake()
    {
        _volumeSlider = gameObject.FindChildWithName<Slider>("Slider");
        _volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        _volumeSlider.value = Volume();
    }

    public static void Load(XmlNode root)
    {
        float volume = root.FloatFromNode("Volume");
        SetMasterVolume(volume);
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Volume", _masterVolume);
    }

    public static float Volume()
    {
        return _masterVolume;
    }

    public static float ModifiedVolume()
    {
        return _modifiedVolume;
    }

    private static float NormalisedVolumeToAttenuation(float volume)
    {
        if (volume == 0) volume = 0.001f;
        return Mathf.Log(volume) * 20f;
    }

    private static void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
        _audioMixer.SetFloat("Master", NormalisedVolumeToAttenuation(_masterVolume));
        SaveController.SaveSettings();
    }

    public static void SetModifiedVolume(float volume)
    {
        _modifiedVolume = volume;
        if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
        _audioMixer.SetFloat("Modified", NormalisedVolumeToAttenuation(_modifiedVolume));
    }
}