﻿using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    private Slider _slider;
    private ParticleSystem _healthParticles;
    private RectTransform _sliderRect;
    private Transform _healthParticleTransform;

    public void Awake()
    {
        _slider = GetComponent<Slider>();
        _healthParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Health Effect");
        _healthParticleTransform = _healthParticles.transform;
        _sliderRect = _slider.GetComponent<RectTransform>();
    }
    // write better comments
    public void SetValue(float value)
    {
        if (value != 1)
        {
            SetParticleEmissionOverDistance(50);
        }
        _slider.value = value;
        float width = _sliderRect.rect.width / 2;
        value -= 0.5f;
        value *= 2;
        if (_slider.direction == Slider.Direction.RightToLeft)
        {
            value = -value;
        }
        float position = value * width;
        _healthParticleTransform.localPosition = new Vector3(position, 0, 0);
    }

    public void SetColor(Color color)
    {
        ParticleSystem.MainModule main = _healthParticles.main;
        main.startColor = color;
    }

    public void SetParticleEmissionOverDistance(float amount = 0)
    {
        ParticleSystem.EmissionModule main = _healthParticles.emission;
        main.rateOverDistance = amount;
    }
}