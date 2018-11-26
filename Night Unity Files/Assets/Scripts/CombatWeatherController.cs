using Game.Combat.Generation;
using Game.Exploration.Regions;
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

        SetParticleSystemEmissionRate(_hail, weatherAttributes.HailAmount, HailMax);
        SetParticleSystemEmissionRate(_rain, weatherAttributes.RainAmount, RainMax);
        SetParticleSystemEmissionRate(_wind, weatherAttributes.WindAmount, WindMax);
        SetParticleSystemEmissionRate(_sun, weatherAttributes.SunAmount, SunMax);
        SetParticleSystemEmissionRate(_dust, weatherAttributes.DustAmount, DustMax);
        SetParticleSystemEmissionRate(_fog, weatherAttributes.FogAmount, FogMax);
    }

    private static void SetParticleSystemEmissionRate(ParticleSystem ps, float amount, float max)
    {
        float emissionRate = amount * max;
        if (CombatManager.Region().GetRegionType() == RegionType.Rite) emissionRate = 0f;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = emissionRate;
        ps.Play();
    }
}