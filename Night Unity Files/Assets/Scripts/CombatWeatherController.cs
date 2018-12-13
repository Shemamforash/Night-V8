using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Game.Exploration.Weather;
using SamsHelper.Libraries;
using UnityEngine;

public class CombatWeatherController : MonoBehaviour
{
    private ParticleSystem _hail, _rain, _wind, _sun, _dust, _fog;
    private WeatherAttributes _weatherAttributes;
    private const float HailMax = 300f, RainMax = 500f, WindMax = 50f, SunMax = 100f, DustMax = 400f, FogMax = 20f;
    private float _windDirection;

    public void Awake()
    {
        Weather currentWeather = WeatherManager.CurrentWeather();
        _weatherAttributes = currentWeather.Attributes;
        _hail = gameObject.FindChildWithName<ParticleSystem>("Hail");
        _rain = gameObject.FindChildWithName<ParticleSystem>("Rain");
        _wind = gameObject.FindChildWithName<ParticleSystem>("Wind");
        _sun = gameObject.FindChildWithName<ParticleSystem>("Sun");
        _dust = gameObject.FindChildWithName<ParticleSystem>("Dust");
        _fog = gameObject.FindChildWithName<ParticleSystem>("Fog");
        Debug.Log(_weatherAttributes.RainAmount);

        SetParticleSystemEmissionRate(_hail, _weatherAttributes.HailAmount, HailMax);
        SetParticleSystemEmissionRate(_rain, _weatherAttributes.RainAmount, RainMax);
        SetParticleSystemEmissionRate(_wind, _weatherAttributes.WindAmount, WindMax);
        SetParticleSystemEmissionRate(_sun, _weatherAttributes.SunAmount, SunMax);
        SetParticleSystemEmissionRate(_dust, _weatherAttributes.DustAmount, DustMax);
        SetParticleSystemEmissionRate(_fog, _weatherAttributes.FogAmount, FogMax);
    }

    private static void SetParticleSystemEmissionRate(ParticleSystem ps, float amount, float max)
    {
        float emissionRate = amount * max;
        if (CombatManager.Region().GetRegionType() == RegionType.Rite) emissionRate = 0f;
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = emissionRate;
        ps.Play();
    }

    private void Update()
    {
        if (_weatherAttributes.WindAmount == 0f) return;
        if (!CombatManager.IsCombatActive()) return;
        _windDirection += Random.Range(-5f, 5f);
        Vector2 windForce = AdvancedMaths.CalculatePointOnCircle(_windDirection, 1, Vector2.zero);
        windForce *= _weatherAttributes.WindAmount / 2f;
        List<CanTakeDamage> charactersInRange = CombatManager.GetCharactersInRange(PlayerCombat.Instance.transform.position, 10);
        charactersInRange.ForEach(c =>
        {
            if (!(c is CharacterCombat character)) return;
            character.MovementController.KnockBack(windForce);
        });
    }
}