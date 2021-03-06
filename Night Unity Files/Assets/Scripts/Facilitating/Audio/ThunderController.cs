﻿using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Weather;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Facilitating.Audio
{
	public class ThunderController : MonoBehaviour
	{
		private const  float             LightningDuration = 0.15f;
		private static ThunderController _instance;
		private        ParticleSystem    _lightningSystem;

		private AudioSource thunderSource;
		private float       lightningTimer;
		private bool        _waitingForThunder;

		public  Image lightningImage;
		private bool  _inCombat;

		public void Awake()
		{
			thunderSource = GetComponent<AudioSource>();
			_instance     = this;
			_inCombat     = SceneManager.GetActiveScene().name == "Combat";
		}

		public void Thunder(float volOffset = 0f)
		{
			if (thunderSource == null) return;
			thunderSource.volume = Random.Range(0.9f - volOffset, 1f - volOffset);
			thunderSource.pitch  = Random.Range(0.8f,             1f);
			thunderSource.PlayOneShot(AudioClips.ThunderSounds.RandomElement(), Random.Range(0.6f, 1f));
		}

		private IEnumerator LightningFlash(float waitTime = 0f)
		{
			yield return new WaitForSeconds(waitTime);
			Thunder();
			lightningTimer = LightningDuration;
			while (lightningTimer > 0f)
			{
				lightningTimer += Random.Range(-0.01f, 0.005f);
				if (lightningTimer < 0f) lightningTimer = 0f;
				float opacity                           = 1 / LightningDuration * lightningTimer;
				lightningImage.color = new Color(1f, 1f, 1f, opacity);
				yield return null;
			}
		}

		public static ThunderController Instance() => _instance;

		public void Strike(bool flashOnly)
		{
			if (_inCombat)
			{
				Vector2 firePosition = WorldGrid.GetCellNearMe(PlayerCombat.Instance.CurrentCell(), 12f).Position;
				if (!flashOnly) FireBurstBehaviour.Create(firePosition);
				_instance.StartCoroutine(_instance.LightningFlash());
			}
			else
			{
				if (!flashOnly) WeatherSystemController.TriggerLightning();
				_instance.StartCoroutine(_instance.LightningFlash(1.5f));
			}
		}

		public void Update()
		{
			if (!ShouldUpdate()) return;
			UpdateThunder();
		}

		private bool ShouldUpdate()
		{
			bool notInCombat = CombatManager.Instance() == null || !CombatManager.Instance().IsCombatActive();
			return !(_inCombat && notInCombat);
		}

		private void UpdateThunder()
		{
			if (WeatherManager.CurrentWeather().Thunder == 0) return;
			if (PlayerCombat.Instance != null && !PlayerCombat.Instance.Player.TravelAction.GetCurrentRegion().IsDynamic()) return;
			if (_waitingForThunder) return;
			StartCoroutine(ThunderStrike());
		}

		private IEnumerator ThunderStrike()
		{
			_waitingForThunder = true;
			float pause           = 4 - WeatherManager.CurrentWeather().Thunder;
			bool  combatActive    = CombatManager.Instance() != null && CombatManager.Instance().IsCombatActive();
			float pauseMultiplier = combatActive ? 15f : 5f;
			pause *= pauseMultiplier;
			pause =  Random.Range(0.75f * pause, 1.25f * pause);
			while (pause > 0f)
			{
				if (ShouldUpdate())
				{
					pause -= Time.deltaTime;
				}

				yield return null;
			}

			if (WeatherManager.CurrentWeather().Thunder != 0) Strike(false);
			_waitingForThunder = false;
		}
	}
}