using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Generation;
using Game.Global;
using Extensions;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
	public class DecayBehaviour : MonoBehaviour
	{
		private const           int                        EmitCount  = 100;
		private static readonly ObjectPool<DecayBehaviour> _decayPool = new ObjectPool<DecayBehaviour>("Decay Areas", "Prefabs/Combat/Effects/Decay Area");
		private                 AudioSource                _audioSource;
		private                 DecayDamageDeal            _decayDamage;
		private                 SpriteRenderer             _impact;
		private                 float                      _impactValue;
		private                 ParticleSystem             _shardParticles, _burstParticles, _thunderParticles;

		private void Awake()
		{
			_shardParticles   = gameObject.FindChildWithName<ParticleSystem>("Shards");
			_burstParticles   = gameObject.FindChildWithName<ParticleSystem>("Burst");
			_thunderParticles = gameObject.FindChildWithName<ParticleSystem>("Thunder");
			_decayDamage      = GetComponent<DecayDamageDeal>();
			_impact           = gameObject.FindChildWithName<SpriteRenderer>("Impact");
			_audioSource      = GetComponent<AudioSource>();
			_audioSource.clip = AudioClips.ShatterExplosion;
		}

		public static DecayBehaviour Create(Vector3 position)
		{
			DecayBehaviour decayBehaviour = _decayPool.Create();
			decayBehaviour.Initialise(position);
			return decayBehaviour;
		}

		private void Initialise(Vector3 position)
		{
			_shardParticles.randomSeed = (uint) Random.Range(uint.MinValue, uint.MaxValue);

			transform.position = position;
			_decayDamage.Clear();
			SetImpactValue(0.5f);
			Sequence sequence = DOTween.Sequence();
			sequence.AppendInterval(0.2f);
			sequence.AppendCallback(() =>
			{
				SetImpactValue(1f);
				_burstParticles.Emit(EmitCount);
				_shardParticles.Emit(EmitCount);
				_thunderParticles.Emit(15);
				_audioSource.Play();
			});
			sequence.Append(DOTween.To(GetImpactValue, SetImpactValue, 0f, 2.5f));
			sequence.AppendCallback(() => StartCoroutine(Fade()));
		}

		private void SetImpactValue(float value)
		{
			_impactValue = value;
			_impact.material.SetFloat("_Cutoff", _impactValue);
		}

		private float GetImpactValue() => _impactValue;

		public void AddIgnoreTarget(CanTakeDamage _ignoreTarget)
		{
			_decayDamage.AddIgnoreTarget(_ignoreTarget);
		}

		private IEnumerator Fade()
		{
			while (_shardParticles.particleCount > 0)
			{
				bool active = CombatManager.Instance().IsCombatActive();
				if (!CombatManager.Instance().IsCombatActive() && active)
				{
					_shardParticles.PauseParticles();
					_burstParticles.PauseParticles();
				}
				else if (CombatManager.Instance().IsCombatActive() && !active)
				{
					_shardParticles.ResumeParticles();
					_burstParticles.ResumeParticles();
				}

				yield return null;
			}

			_decayPool.Return(this);
		}

		private void OnDestroy()
		{
			_decayPool.Dispose(this);
		}

		public void AddIgnoreTargets(List<CanTakeDamage> targetsToIgnore)
		{
			_decayDamage.AddIgnoreTargets(targetsToIgnore);
		}
	}
}