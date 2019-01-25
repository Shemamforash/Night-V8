using System.Xml;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour, ISelectHandler, IDeselectHandler, IInputListener
{
    private static float _masterVolume = 1;
    private static float _modifiedVolume;
    private Slider _volumeSlider;


    private void OnEnable()
    {
        _volumeSlider.value = _masterVolume;
    }

    private void Awake()
    {
        _volumeSlider = GetComponent<Slider>();
        _volumeSlider.onValueChanged.AddListener(f => SetMasterVolume(_volumeSlider.value));
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
        return _modifiedVolume;
    }

    private static void SetMasterVolume(float volumeLevel)
    {
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
        InputHandler.RegisterInputListener(this);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        InputHandler.UnregisterInputListener(this);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.Horizontal) return;
        float volumeDifference = 0.5f * Time.deltaTime;
        volumeDifference *= direction.Polarity();
        _volumeSlider.value = _volumeSlider.value + volumeDifference;
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}