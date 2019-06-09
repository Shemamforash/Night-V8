using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies.Misc;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Weather;
using Extensions;
using UnityEngine;

public class CombatWeatherController : MonoBehaviour
{
	private const float             HailMax = 300f, RainMax = 500f, WindMax = 200f, SunMax = 100f, DustMax = 400f, FogMax = 20f;
	private       ParticleSystem    _hail,          _rain,          _wind,          _heavyWind,    _sun,           _dust, _fog;
	private       WeatherAttributes _weatherAttributes;

	public void Awake()
	{
		if (!CharacterManager.CurrentRegion().IsDynamic())
		{
			Destroy(this);
			return;
		}

		Weather currentWeather = WeatherManager.CurrentWeather();
		_weatherAttributes = currentWeather.Attributes;
		_hail              = gameObject.FindChildWithName<ParticleSystem>("Hail");
		_rain              = gameObject.FindChildWithName<ParticleSystem>("Rain");
		_wind              = gameObject.FindChildWithName<ParticleSystem>("Wind");
		_heavyWind         = gameObject.FindChildWithName<ParticleSystem>("Heavy Wind");
		_sun               = gameObject.FindChildWithName<ParticleSystem>("Sun");
		_dust              = gameObject.FindChildWithName<ParticleSystem>("Dust");
		_fog               = gameObject.FindChildWithName<ParticleSystem>("Fog");

		SetParticleSystemEmissionRate(_hail,      _weatherAttributes.HailAmount, HailMax);
		SetParticleSystemEmissionRate(_rain,      _weatherAttributes.RainAmount, RainMax);
		SetParticleSystemEmissionRate(_wind,      _weatherAttributes.WindAmount, WindMax);
		SetParticleSystemEmissionRate(_heavyWind, _weatherAttributes.WindAmount, WindMax / 2f);
		SetParticleSystemEmissionRate(_sun,       _weatherAttributes.SunAmount           * WeatherSystemController.SunLevel(), SunMax);
		SetParticleSystemEmissionRate(_dust,      _weatherAttributes.DustAmount,                                               DustMax);
		SetParticleSystemEmissionRate(_fog,       _weatherAttributes.FogAmount,                                                FogMax);
	}

	private static void SetParticleSystemEmissionRate(ParticleSystem ps, float amount, float max)
	{
		float                         emissionRate = amount * max;
		ParticleSystem.EmissionModule emission     = ps.emission;
		emission.rateOverTime = emissionRate;
		ps.Play();
	}

	private void Update()
	{
		if (_weatherAttributes.WindAmount == 0f) return;
		if (!CombatManager.Instance().IsCombatActive()) return;
		Vector2 windForce = Vector2.up * Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0);
		windForce *= _weatherAttributes.WindAmount * 0.2f;
		List<CanTakeDamage> charactersInRange = CombatManager.Instance().GetCharactersInRange(PlayerCombat.Position(), 10);
		charactersInRange.ForEach(c =>
		{
			if (!(c is CharacterCombat character)) return;
			character.MovementController.KnockBack(windForce);
		});
		ShotManager.Shots().ForEach(s => { s.RigidBody2D().AddForce(windForce * 0.1f); });
		Grenade.Grenades().ForEach(g => { g.RigidBody2D().AddForce(windForce  * 0.5f); });
	}
}