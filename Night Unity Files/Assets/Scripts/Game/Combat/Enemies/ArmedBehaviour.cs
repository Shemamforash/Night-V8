using Game.Combat.Generation;
using Game.Gear.Weapons;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies
{
	public class ArmedBehaviour : UnarmedBehaviour
	{
		private float               _aimTime;
		private Cell                _coverCell;
		private float               _fireTime;
		private bool                _waitingForHeal;
		private BaseWeaponBehaviour _weaponBehaviour;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			Assert.IsNotNull(Weapon());
			_weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
			CalculateMaxMinDistance();
			_aimTime  = Random.Range(0.5f, 1f);
			_fireTime = 2f;
			TryFire();
		}

		protected void CalculateMaxMinDistance()
		{
			MinDistance = Weapon().CalculateMinimumDistance();
			MaxDistance = MinDistance * 2f;
			if (MaxDistance > 4f) MaxDistance = 4f;
		}

		protected virtual void TryFire()
		{
			Aim();
		}

		public override Weapon Weapon() => Enemy.Weapon;

		public override void MyUpdate()
		{
			base.MyUpdate();
			if (_aimTime > 0f) _aimTime -= Time.deltaTime;
		}

		public override string GetDisplayName() => Enemy.Template.DisplayName;

		private void Reload()
		{
			float duration = Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * 2f;
			CurrentAction = () =>
			{
				duration -= Time.deltaTime;
				if (duration > 0) return;
				_weaponBehaviour.Reload();
				TryFire();
			};
		}

		protected virtual void Aim()
		{
			CurrentAction = () =>
			{
				_fireTime = 2f;
				if (_aimTime > 0f) return;
				if (_weaponBehaviour.Empty())
				{
					Reload();
				}
				else
				{
					CurrentAction = Fire;
				}
			};
		}

		private void Fire()
		{
			Transform enemyTransform;
			bool      outOfRange = (enemyTransform = transform).Distance(GetTarget().transform) > MaxDistance;
			bool      outOfSight = outOfRange || Physics2D.Linecast(enemyTransform.position, GetTarget().transform.position, 1 << 8).collider != null;
			if (outOfSight)
			{
				TryFire();
				return;
			}

			_fireTime -= Time.deltaTime;
			if (_fireTime <= 0)
			{
				_weaponBehaviour.StopFiring();
				_aimTime = Random.Range(0.5f, 1f);
				Aim();
				return;
			}

			if (!_weaponBehaviour.CanFire())
			{
				if (!_weaponBehaviour.Empty()) return;
				_weaponBehaviour.StopFiring();
				Aim();
				return;
			}

			_weaponBehaviour.StartFiring();
		}
	}
}