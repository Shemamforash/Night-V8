using System.Collections;
using DG.Tweening;
using Fastlights;
using Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
	public class FireBehaviour : MonoBehaviour
	{
		private static readonly ObjectPool<FireBehaviour> _firePool = new ObjectPool<FireBehaviour>("Fire Areas", "Prefabs/Combat/Effects/Fire Area");
		private                 bool                      _fading;
		private                 ParticleSystem            _flames;
		private                 FastLight                 _light;
		private                 float                     _randomSeed;

		public void Awake()
		{
			_flames     = GetComponent<ParticleSystem>();
			_light      = gameObject.FindChildWithName<FastLight>("Light");
			_randomSeed = Random.Range(0f, 5f);
		}

		public static FireBehaviour Create(Vector3 position)
		{
			FireBehaviour fire = _firePool.Create();
			fire.Initialise(position);
			return fire;
		}

		public void Update()
		{
			if (_fading) return;
			float brightness = Mathf.PerlinNoise(Time.time * 2, _randomSeed) * 0.25f + 0.05f;
			_light.SetAlpha(brightness);
		}

		private void Initialise(Vector3 position)
		{
			transform.position = position;
			_flames.Play();
			_fading = false;
		}

		public void LetDie()
		{
			_flames.Stop();
			_fading = true;
			Sequence sequence = DOTween.Sequence();
			sequence.Append(DOTween.To(_light.GetAlpha, _light.SetAlpha, 0f, 1f));
			sequence.AppendCallback(() => _firePool.Return(this));
		}

		private void OnDestroy()
		{
			_firePool.Dispose(this);
		}

		public void SetLifeTime(float lifeTime)
		{
			StartCoroutine(WaitThenDie(lifeTime));
		}

		private IEnumerator WaitThenDie(float lifeTime)
		{
			while (lifeTime > 0)
			{
				lifeTime -= Time.deltaTime;
				yield return null;
			}

			LetDie();
		}
	}
}