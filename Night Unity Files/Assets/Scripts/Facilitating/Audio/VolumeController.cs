using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private static float _masterVolume = 1;
    private static float _modifiedVolume;
    private Slider _volumeSlider;
    private static bool _loaded;

    private void OnEnable()
    {
        _volumeSlider.value = _masterVolume;
    }

    private void Awake()
    {
        _volumeSlider = GetComponent<Slider>();
        _volumeSlider.value = _masterVolume;
        _volumeSlider.onValueChanged.AddListener(f => SetMasterVolume(_volumeSlider.value));
    }

    private void Start()
    {
        _volumeSlider.value = _masterVolume;
    }

    public static void Load(XmlNode root)
    {
        _loaded = true;
        float volume = root.FloatFromNode("Volume");
        SetMasterVolume(volume);
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Volume", _masterVolume);
    }

    public static float Volume()
    {
        return _modifiedVolume;
    }

    private static void SetMasterVolume(float volumeLevel)
    {
        if (!_loaded) return;
        _masterVolume = volumeLevel;
        AudioController.SetMasterVolume(_masterVolume);
        SaveController.SaveSettings();
    }

    public static void SetModifiedVolume(float volume)
    {
        _modifiedVolume = volume;
        AudioController.SetModifiedVolume(_modifiedVolume);
    }

    public void OnSelect(BaseEventData eventData)
    {
        EnhancedButton.DeselectCurrent();
    }

    public void OnDeselect(BaseEventData eventData)
    {
    }

    public static void SetToDefaultVolume()
    {
        _loaded = true;
        SetMasterVolume(1);
    }
}