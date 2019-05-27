using Extensions;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
	public class BulletTrailController : AbstractBulletTrail
	{
		private static readonly ObjectPool<AbstractBulletTrail> _pool = new ObjectPool<AbstractBulletTrail>("Void Trails", "Prefabs/Combat/Shots/Trail");

		[SerializeField] private ParticleSystem _void;
		[SerializeField] private ParticleSystem _fire;
		[SerializeField] private ParticleSystem _shatter;
		[SerializeField] private ParticleSystem _rifle;
		[SerializeField] private ParticleSystem _shotgun;
		[SerializeField] private ParticleSystem _smg;
		[SerializeField] private TrailRenderer  _default;

		public static BulletTrailController Create(bool isPlayer, ShotAttributes shotAttributes)
		{
			BulletTrailController trail = (BulletTrailController) _pool.Create();
			trail.Initialise(isPlayer, shotAttributes);
			return trail;
		}

		private void Initialise(bool isPlayer, ShotAttributes shotAttributes)
		{
			Weapon weapon = shotAttributes.Weapon;

			Color trailColor = isPlayer ? Color.white : Color.red;
			_default.startColor = trailColor;
			_default.endColor   = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
			_shotgun.SetStartColour(trailColor);
			_smg.SetStartColour(trailColor);

			_rifle.SetColourOverLifetime(trailColor.ChangeAlpha(0.5f), trailColor.ChangeAlpha(0.8f));

			_shotgun.SetEmissionRateOverDistance((int) weapon.Val(AttributeType.Pellets));

			int   smgParticles              = 0;
			float fireRate                  = weapon.Val(AttributeType.FireRate);
			if (fireRate > 4f) smgParticles = (int) (fireRate * 2);
			_smg.SetEmissionRateOverDistance(smgParticles, smgParticles * 2);

			int   rifleParticles             = 0;
			float range                      = weapon.Val(AttributeType.Accuracy);
			if (range > 0.5f) rifleParticles = (int) (30 * range);
			_rifle.SetEmissionRateOverDistance(rifleParticles);

			_void.SetEmissionRateOverDistance(shotAttributes.WillVoid ? 20 : 0);
			_fire.SetEmissionRateOverDistance(shotAttributes.WillBurn ? 4 : 0, shotAttributes.WillBurn ? 8 : 0);
			_shatter.SetEmissionRateOverDistance(shotAttributes.WillShatter ? 5 : 0);
		}

		protected override bool Done()
		{
			return _void.particleCount    == 0
			    && _fire.particleCount    == 0
			    && _shatter.particleCount == 0
			    && _shotgun.particleCount == 0
			    && _default.positionCount == 0
			    && _smg.particleCount     == 0
			    && _rifle.particleCount   == 0;
		}

		private            void                            OnDestroy()     => _pool.Dispose(this);
		protected override ObjectPool<AbstractBulletTrail> GetObjectPool() => _pool;

		protected override void ClearTrails()
		{
			_void.Clear();
			_fire.Clear();
			_shatter.Clear();
			_rifle.Clear();
			_shotgun.Clear();
			_smg.Clear();
			_default.Clear();
		}
	}
}