using System;
using Game.Exploration.Weather;
using SamsHelper.Libraries;
using UnityEngine;

public class CombatWeatherController : MonoBehaviour
{
    private ParticleSystem _hail, _rain, _wind, _sun, _dust, _fog;
    private const float HailMax = 200f, RainMax = 10f, WindMax = 20f, SunMax = 100f, DustMax = 300f, FogMax = 20f;

    public void Awake()
    {
        Weather currentWeather = WeatherManager.CurrentWeather();
        WeatherAttributes weatherAttributes = currentWeather.Attributes;
        _hail = gameObject.FindChildWithName<ParticleSystem>("Hail");
        _rain = gameObject.FindChildWithName<ParticleSystem>("Rain");
        _wind = gameObject.FindChildWithName<ParticleSystem>("Wind");
        _sun = gameObject.FindChildWithName<ParticleSystem>("Sun");
        _dust = gameObject.FindChildWithName<ParticleSystem>("Dust");
        _fog = gameObject.FindChildWithName<ParticleSystem>("Fog");

        SetParticleSystemEmissionRate(_hail, weatherAttributes.HailAmount, HailMax, null);
        SetParticleSystemEmissionRate(_rain, weatherAttributes.RainAmount, RainMax, WeatherSystemController.GetRainClip);
        SetParticleSystemEmissionRate(_wind, weatherAttributes.WindAmount, WindMax, WeatherSystemController.GetWindClip);
        SetParticleSystemEmissionRate(_sun, weatherAttributes.SunAmount, SunMax, null);
        SetParticleSystemEmissionRate(_dust, weatherAttributes.DustAmount, DustMax, null);
        SetParticleSystemEmissionRate(_fog, weatherAttributes.FogAmount, FogMax, null);
    }


    private static void SetParticleSystemEmissionRate(ParticleSystem ps, float strength, float max, Func<float, AudioClip> getAudioClip)
    {
        float amount = strength * max;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = amount;
        if (amount != 0)
        {
            ps.Play();
            AudioSource audioSource = ps.GetComponent<AudioSource>();
            audioSource.clip = getAudioClip?.Invoke(strength);
            audioSource.Play();
        }
    }
}