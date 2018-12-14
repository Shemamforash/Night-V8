using System;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    private EnhancedButton _silentButton, _hushedButton, _loudButton;
    private Image _silentBackground, _hushedBackground, _loudBackground;
    private static VolumeLevel _masterVolume = VolumeLevel.Loud;
    private static float _modifiedVolume;

    private enum VolumeLevel
    {
        Mute,
        Hushed,
        Loud
    }

    private void OnEnable()
    {
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        float targetSilentAlpha = _masterVolume == VolumeLevel.Mute ? 0.5f : 0f;
        float targetHushedAlpha = _masterVolume == VolumeLevel.Hushed ? 0.5f : 0f;
        float targetLoudAlpha = _masterVolume == VolumeLevel.Loud ? 0.5f : 0f;
        _silentBackground.DOFade(targetSilentAlpha, 0.5f);
        _hushedBackground.DOFade(targetHushedAlpha, 0.5f);
        _loudBackground.DOFade(targetLoudAlpha, 0.5f);
    }

    private void Awake()
    {
        _silentButton = gameObject.FindChildWithName("Mute").FindChildWithName<EnhancedButton>("Button");
        _silentButton.AddOnClick(() =>
        {
            SetMasterVolume(VolumeLevel.Mute);
            UpdateBackground();
        });
        _silentBackground = _silentButton.FindChildWithName<Image>("Image");

        _hushedButton = gameObject.FindChildWithName("Hushed").FindChildWithName<EnhancedButton>("Button");
        _hushedButton.AddOnClick(() =>
        {
            SetMasterVolume(VolumeLevel.Hushed);
            UpdateBackground();
        });
        _hushedBackground = _hushedButton.FindChildWithName<Image>("Image");

        _loudButton = gameObject.FindChildWithName("Loud").FindChildWithName<EnhancedButton>("Button");
        _loudButton.AddOnClick(() =>
        {
            SetMasterVolume(VolumeLevel.Loud);
            UpdateBackground();
        });
        _loudBackground = _loudButton.FindChildWithName<Image>("Image");
    }

    public static void Load(XmlNode root)
    {
        VolumeLevel volume = (VolumeLevel) root.IntFromNode("Volume");
        SetMasterVolume(volume);
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Volume", (int) _masterVolume);
    }

    public static float Volume()
    {
        return _modifiedVolume;
    }

    private static void SetMasterVolume(VolumeLevel volumeLevel)
    {
        _masterVolume = volumeLevel;
        AudioController.SetMasterVolume(VolumeLevelToValue());
        SaveController.SaveSettings();
    }

    private static float VolumeLevelToValue()
    {
        switch (_masterVolume)
        {
            case VolumeLevel.Mute:
                return 0f;
            case VolumeLevel.Hushed:
                return 0.5f;
            case VolumeLevel.Loud:
                return 1f;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void SetModifiedVolume(float volume)
    {
        _modifiedVolume = volume;
        AudioController.SetModifiedVolume(_modifiedVolume);
    }
}