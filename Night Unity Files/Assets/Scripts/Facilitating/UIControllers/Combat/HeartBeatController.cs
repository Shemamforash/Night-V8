using Game.Combat.Player;
using Game.Global;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class HeartBeatController : MonoBehaviour
	{
		private readonly float               _heartBeatLongGap  = 1f;
		private readonly float               _heartBeatShortGap = 0.25f;
		private          AudioPoolController _audioPoolController;
		private          float               _currentTime;
		private          ParticleSystem      _heartBeatParticles;
		private          bool                _timingLongBeat;

		public void Awake()
		{
			_heartBeatParticles  = GetComponent<ParticleSystem>();
			_audioPoolController = GetComponent<AudioPoolController>();
			_audioPoolController.SetMixerGroup("Master", 0);
		}

		public void Start()
		{
			SetTrailAlpha(0f);
		}

		private void TryPlayParticles(float health)
		{
			if (health <= 0.4f)
			{
				if (!_heartBeatParticles.isPlaying) _heartBeatParticles.Play();
			}
			else if (_heartBeatParticles.isPlaying)
			{
				_heartBeatParticles.Stop();
			}
		}

		private void OnDestroy()
		{
			AudioController.SetGlobalMuffle(22000);
		}

		private void UpdateMuffle(float health)
		{
			float muffle = health > 0.4f ? 1 : health / 0.4f;
			muffle = Mathf.Lerp(500, 22000, muffle    * muffle);
			AudioController.SetGlobalMuffle(muffle);
		}

		public void Update()
		{
			float health = PlayerCombat.Instance.HealthController.GetNormalisedHealthValue();
			TryPlayParticles(health);
			UpdateMuffle(health);
			if (health >= 0.4f) return;
			float maxAlpha = 1f - health / 0.4f;
			float minAlpha = Mathf.Clamp(maxAlpha - 0.6f, 0f, 1f);
			float currentAlpha;
			_currentTime += Time.deltaTime;
			float volume = 1 - maxAlpha;
			if (_timingLongBeat)
			{
				if (_currentTime >= _heartBeatLongGap)
				{
					InstancedAudio instancedAudio = _audioPoolController.Create();
					instancedAudio.Play(AudioClips.ShortHeartBeat, volume, Random.Range(0.9f, 1f));
					_timingLongBeat =  false;
					_currentTime    -= _heartBeatLongGap;
				}

				currentAlpha = 1f - _currentTime / _heartBeatLongGap;
			}
			else
			{
				if (_currentTime >= _heartBeatShortGap)
				{
					InstancedAudio instancedAudio = _audioPoolController.Create();
					instancedAudio.Play(AudioClips.LongHeartBeat, volume, Random.Range(0.9f, 1f));
					_timingLongBeat =  true;
					_currentTime    -= _heartBeatShortGap;
				}

				currentAlpha = 1f - _currentTime / _heartBeatShortGap;
			}

			if (currentAlpha < 0) currentAlpha = 0f;
			currentAlpha *= maxAlpha - minAlpha;
			currentAlpha += minAlpha;
			SetTrailAlpha(currentAlpha);
		}

		private void SetTrailAlpha(float alpha)
		{
			ParticleSystem.TrailModule trails = _heartBeatParticles.trails;
			trails.colorOverTrail = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha), UiAppearanceController.InvisibleColour);
		}
	}
}