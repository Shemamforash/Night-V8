using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Combat.Enemies
{
	public class ArmedBehaviour : UnarmedBehaviour
	{
		private bool                _waitingForHeal;
		private BaseWeaponBehaviour _weaponBehaviour;
		private Cell                _coverCell;
		private float               _aimTime;
		private bool                _automatic;
		private float               _repositionTime = -1;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			Assert.IsNotNull(Weapon());
			_weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
			CalculateMaxMinDistance();
			ResetAimTime();
			_automatic = Weapon().WeaponAttributes.Automatic;
			TryFire();
		}

		protected void CalculateMaxMinDistance()
		{
			MaxDistance = Weapon().WeaponAttributes.Range() - 0.25f;
			if (MaxDistance > 4f) MaxDistance = 4f;
			MinDistance = 1f;
		}

		protected virtual void TryFire()
		{
			Aim();
		}

		private void ResetAimTime()
		{
			_aimTime = Random.Range(0.25f, 0.5f);
		}

		public override Weapon Weapon() => Enemy.Weapon;

		public override string GetDisplayName()
		{
			return Enemy.Template.DisplayName;
		}

		private void Reload()
		{
			float duration = Weapon().WeaponAttributes.ReloadSpeed();
			CurrentAction = () =>
			{
				duration -= Time.deltaTime;
				if (duration > 0) return;
				_weaponBehaviour.Reload();
				ResetAimTime();
				TryFire();
			};
		}

		protected virtual void Aim()
		{
			CurrentAction = () =>
			{
				if (_weaponBehaviour.Empty()) Reload();
				_aimTime -= Time.deltaTime;
				if (_aimTime > 0f) return;
				CurrentAction = Fire;
			};
		}

		private void Fire()
		{
			Transform transform1;
			bool      outOfRange = (transform1 = transform).Distance(GetTarget().transform) > MaxDistance;
			bool      outOfSight = outOfRange || Physics2D.Linecast(transform1.position, GetTarget().transform.position, 1 << 8).collider != null;

			if (outOfSight)
			{
				_aimTime = Random.Range(0.25f, 0.5f);
				TryFire();
				return;
			}

			if (_weaponBehaviour.Empty())
			{
				_weaponBehaviour.StopFiring();
				Reload();
				return;
			}

			if (_automatic)
			{
				if (!_weaponBehaviour.FireRateTargetMet()) return;
				_weaponBehaviour.StartFiring();
				return;
			}

			if (_weaponBehaviour.Fired)
			{
				_weaponBehaviour.StopFiring();
				_aimTime = Random.Range(0.2f, 0.3f);
				Aim();
				return;
			}

			_weaponBehaviour.StartFiring();
		}

		protected override void UpdateTargetCell()
		{
			if (GetTarget() == null) SetTarget(PlayerCombat.Instance);
			if (GetTarget() == null) return;
			_repositionTime -= Time.deltaTime;
			if (_repositionTime > 0f) return;
			_repositionTime = Random.Range(1f, 3f);
			Cell       playerCell  = ((CharacterCombat) GetTarget()).CurrentCell();
			List<Cell> cellsNearMe = WorldGrid.GetCellsNearMe(playerCell.Position, 30, MaxDistance);
			if (cellsNearMe.Count == 0) return;
			foreach (Cell cell in cellsNearMe)
			{
				bool outOfSight = Physics2D.Linecast(playerCell.Position, cell.Position, 1 << 8).collider != null;
				if (outOfSight) continue;
				Vector2 difference           = cell.Position - playerCell.Position;
				float   distanceToTargetCell = difference.magnitude;
				if (distanceToTargetCell < MinDistance) return;
				TargetCell = cell;
				break;
			}
		}
	}
}