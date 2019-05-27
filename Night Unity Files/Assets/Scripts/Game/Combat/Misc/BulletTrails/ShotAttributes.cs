using System;
using System.Collections.Generic;
using Extensions;
using Game.Combat.Enemies;
using Game.Combat.Player;
using Game.Exploration.Weather;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
	public class ShotAttributes
	{
		private const float MinimumAccuracyOffsetInDegrees = 20f;
		private const float MaximumAccuracyOffsetInDegrees = 40f;
		private const float MinAge                         = 0.25f;
		private const float MaxAge                         = 2f;
		private const float SeekDecay                      = 0.95f;

		private readonly CharacterCombat _origin;
		private          int             _condition = -1;
		private          int             _damage, _damageDealt;
		private          float           _finalDamageModifier = 1f;
		private          bool            _seekTarget;

		private float _speed, _burnChance, _shatterChance, _voidChance, _accuracy, _knockBackForce, _knockBackModifier, _age, _seekModifier = 1;

		private Weapon _weapon;
		public  bool   Fired;
		private float  _shotAge;

		public Weapon Weapon      => _weapon;
		public bool   WillVoid    { get; private set; }
		public bool   WillBurn    { get; private set; }
		public bool   WillShatter { get; private set; }


		public ShotAttributes(CharacterCombat origin)
		{
			_origin = origin;
			SetWeapon(origin.Weapon());
			CacheWeaponAttributes();
		}

		private event Action OnHitAction;

		private void SetWeapon(Weapon weapon)
		{
			_weapon = weapon;
			_speed  = _origin is PlayerCombat ? 10 : 6;
		}

		public float GetSeekForce()
		{
			if (!_seekTarget) return -1;
			float force = 50;
			force         *= _seekModifier;
			_seekModifier *= SeekDecay;
			return force;
		}

		private void CacheWeaponAttributes()
		{
			_damage        = (int) _weapon.Val(AttributeType.Damage);
			_accuracy      = 1 - _weapon.Val(AttributeType.Accuracy);
			_shatterChance = _weapon.CalculateShatterChance();
			_burnChance    = _weapon.CalculateBurnChance();
			_voidChance    = _weapon.CalculateVoidChance();
			_shotAge       = Mathf.Lerp(MinAge, MaxAge, _weapon.Val(AttributeType.Range) / 100f);
			if (!(_origin is PlayerCombat)) return;
			_shatterChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.Shatter);
			_burnChance    += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.Burn);
			_voidChance    += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.Void);
		}

		public float CalculateAccuracy()
		{
			if (_origin == null) return 0;
			if (_origin is EnemyBehaviour) _accuracy *= 1.25f;
			float minAccuracy                        = MinimumAccuracyOffsetInDegrees * _accuracy;
			float maxAccuracy                        = MaximumAccuracyOffsetInDegrees * _accuracy;
			float recoilModifier                     = _origin.GetRecoilModifier();
			float currentAccuracy                    = Mathf.Lerp(minAccuracy, maxAccuracy, recoilModifier);
			return currentAccuracy;
		}

		public float GetSpeed() => _speed;

		public BulletTrailController GetBulletTrail()
		{
			bool isPlayer = _origin is PlayerCombat;
			return BulletTrailController.Create(isPlayer, this);
		}

		public void AddOnHit(Action a)
		{
			OnHitAction += a;
		}

		public void Seek()
		{
			_seekTarget = true;
		}

		public int DamageDealt() => (int) (_damageDealt * _finalDamageModifier);

		private void SetConditions()
		{
			float     random            = Random.Range(0f, 100f);
			float     conditionModifier = _weapon.Val(AttributeType.Pellets) * _weapon.Val(AttributeType.Capacity);
			bool      canDecay          = random < _shatterChance / conditionModifier;
			bool      canBurn           = random < _burnChance    / conditionModifier;
			bool      canVoid           = random < _voidChance    / conditionModifier;
			List<int> conditions        = new List<int>();
			if (canDecay) conditions.Add(0);
			if (canBurn) conditions.Add(1);
			if (canVoid) conditions.Add(2);
			if (conditions.Count == 0) return;
			_condition = conditions.RandomElement();
			switch (_condition)
			{
				case 0:
					WillShatter = true;
					break;
				case 1:
					WillBurn = true;
					break;
				case 2:
					WillVoid = true;
					break;
			}
		}


		public void ApplyConditions(Vector2 position)
		{
			if (_condition == -1) return;
			PlayerCombat player = _origin as PlayerCombat;
			switch (_condition)
			{
				case 0:
					DecayBehaviour.Create(position);
					if (player != null)
					{
						player.Player.BrandManager.IncreaseVoidCount();
						AchievementManager.Instance().IncreaseElementTriggers();
					}

					break;
				case 1:
					FireBurstBehaviour.Create(position);
					if (player != null)
					{
						player.Player.BrandManager.IncreaseBurnCount();
						AchievementManager.Instance().IncreaseElementTriggers();
					}

					break;
				case 2:
					SickenBehaviour.Create(position, new List<CanTakeDamage> {_origin});
					if (player != null)
					{
						player.Player.BrandManager.IncreaseShatterCount();
						AchievementManager.Instance().IncreaseElementTriggers();
					}

					break;
			}
		}

		public void DealDamage(GameObject other, Shot shot)
		{
			_damageDealt = _damage;
			OnHitAction?.Invoke();
			CanTakeDamage hit = other.GetComponent<CanTakeDamage>();
			if (hit == null) return;
			PlayerCombat player = _origin as PlayerCombat;
			CalculateKnockBackForce();
			float previousHealth = hit.HealthController.GetCurrentHealth();
			hit.TakeShotDamage(shot);
			if (player != null) player.OnShotConnects(hit, previousHealth);
			_damage = Mathf.CeilToInt(_damage * 0.5f);
		}

		private void CalculateKnockBackForce()
		{
			float rainModifier = WeatherManager.CurrentWeather().Attributes.RainAmount;
			_knockBackModifier += rainModifier;
			_knockBackForce    =  _damageDealt / 4f * _knockBackModifier;
		}

		public void SetDamageModifier(float modifier)
		{
			_finalDamageModifier = modifier;
		}

		public float GetKnockBackForce() => _knockBackForce;

		public bool UpdateAge()
		{
			_age += Time.deltaTime;
			return _age < _shotAge;
		}

		public void Fire()
		{
			Fired = true;
			SetConditions();
		}

		public bool DidPierce()
		{
			return Random.Range(0f, 1f) < _weapon.PierceChance();
		}
	}
}