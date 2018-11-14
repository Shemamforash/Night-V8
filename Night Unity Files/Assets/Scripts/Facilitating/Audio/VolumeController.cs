using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    private Slider _volumeSlider;
    private static float _masterVolume = 1, _modifiedVolume;

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

    private static void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
        AudioController.SetMasterVolume(_masterVolume);
        SaveController.SaveSettings();
    }

    public static void SetModifiedVolume(float volume)
    {
        _modifiedVolume = volume;
        AudioController.SetModifiedVolume(_modifiedVolume);
    }
}