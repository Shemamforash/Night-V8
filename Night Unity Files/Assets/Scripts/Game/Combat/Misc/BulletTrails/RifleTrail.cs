using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
	public class RifleTrail : BulletTrail
	{
		private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("Rifle Trails", "Prefabs/Combat/Shots/Rifle Trail");
		private                 ParticleSystem          _path;

		public void Awake()
		{
			_path = GetComponent<ParticleSystem>();
		}

		public static RifleTrail Create(bool isPlayer)
		{
			RifleTrail trail = (RifleTrail) _pool.Create();
			trail.Initialise(isPlayer);
			return trail;
		}

		private void Initialise(bool isPlayer)
		{
			ParticleSystem.ColorOverLifetimeModule colorModule = _path.colorOverLifetime;
			if (isPlayer)
			{
				colorModule.color = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 0.5f), new Color(1, 1, 1, 0.8f));
			}
			else
			{
				colorModule.color = new ParticleSystem.MinMaxGradient(new Color(1, 0, 0, 0.5f), new Color(1, 0, 0, 0.8f));
			}
		}

		protected override bool Done() => _path.particleCount == 0;

		protected override ObjectPool<BulletTrail> GetObjectPool() => _pool;

		protected override void ClearTrails()
		{
			_path.Clear();
		}
	}
}