using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Misc
{
	public class SMGTrail : BulletTrail
	{
		private static readonly ObjectPool<BulletTrail> _pool = new ObjectPool<BulletTrail>("SMG Trails", "Prefabs/Combat/Shots/SMG Trail");
		private                 ParticleSystem          _path;

		public void Awake()
		{
			_path = GetComponent<ParticleSystem>();
		}

		public static SMGTrail Create(bool isPlayer)
		{
			SMGTrail trail = (SMGTrail) _pool.Create();
			trail.Initialise(isPlayer);
			return trail;
		}

		protected override bool Done() => _path.particleCount == 0;

		protected override ObjectPool<BulletTrail> GetObjectPool() => _pool;

		protected override void ClearTrails()
		{
			_path.Clear();
		}

		private void Initialise(bool isPlayer)
		{
			ParticleSystem.MainModule main = _path.main;
			main.startColor = isPlayer ? Color.white : Color.red;
		}
	}
}