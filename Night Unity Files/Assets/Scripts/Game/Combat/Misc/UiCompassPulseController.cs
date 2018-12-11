using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class UiCompassPulseController : MonoBehaviour
{
    private static GameObject _pulsePrefab;
    private static Transform _pulseContent;
    private static readonly List<Pulse> _pulses = new List<Pulse>();
    private static RectTransform _rectTransform;

    public void Awake()
    {
        _pulseContent = transform;
        _pulsePrefab = Resources.Load("Prefabs/Combat/Visuals/Compass Pulse") as GameObject;
        _rectTransform = GetComponent<RectTransform>();
    }

    public static RectTransform CompassRect() => _rectTransform;

    public static void InitialisePulses(int max, int current)
    {
        _pulses.ForEach(a => a.Destroy());
        _pulses.Clear();
        for (int i = 0; i < max; ++i)
        {
            Pulse newPulse = new Pulse(Helper.InstantiateUiObject(_pulsePrefab, _pulseContent));
            _pulses.Add(newPulse);
        }

        _pulses.Reverse();
        for (int i = max - 1; i >= current; --i)
        {
            UsePulse(i);
        }
    }

    public static void UsePulse(int pulsesRemaining)
    {
        _pulses[pulsesRemaining].MarkUsed();
        if (pulsesRemaining == 0) return;
        _pulses[pulsesRemaining - 1].MarkReady();
    }

    private class Pulse
    {
        private readonly GameObject _pulseObject;
        private readonly Image _pulseImage, _glowImage;

        public Pulse(GameObject pulseObject)
        {
            Vector3 position = pulseObject.transform.position;
            position.z = 0;
            pulseObject.transform.position = position;
            _pulseObject = pulseObject;
            _pulseImage = pulseObject.GetComponent<Image>();
            _glowImage = pulseObject.FindChildWithName<Image>("Glow");
            _glowImage.color = new Color(1, 1, 1, 0);
        }

        public void MarkUsed()
        {
            _glowImage.color = Color.white;
            _glowImage.DOFade(0f, 1f);
            _pulseImage.DOFade(0.5f, 1f);
        }

        public void MarkReady()
        {
            _glowImage.DOFade(0.5f, 0.5f);
        }

        public void Destroy()
        {
            Object.Destroy(_pulseObject);
        }
    }
}