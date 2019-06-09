using System;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
	public class ArmedBehaviour : UnarmedBehaviour
	{
		private bool                _waitingForHeal;
		private Cell                _coverCell;
		private float               _aimTime;
		private bool                _automatic;
		private float               _repositionTime = -1;
		private float               _reloadDuration;

		public override void Initialise(Enemy enemy)
		{
			base.Initialise(enemy);
			Assert.IsNotNull(Weapon());
			WeaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
			CalculateMaxMinDistance();
			ResetAimTime();
			_automatic      = Weapon().WeaponAttributes.Automatic;
			_reloadDuration = Weapon().WeaponAttributes.ReloadSpeed();
			CurrentAction   = _automatic ? (Action) TryFireAuto : TryFireManual;
		}

		private bool HasLineOfSight()
		{
			Transform enemyTransform  = transform;
			Transform targetTransform = GetTarget().transform;
			bool      outOfRange      = enemyTransform.Distance(targetTransform) > MaxDistance;
			if (outOfRange) return false;
			bool outOfSight = Physics2D.Linecast(enemyTransform.position, targetTransform.position, 1 << 8).collider != null;
			return !outOfSight;
		}

		private void CalculateMaxMinDistance()
		{
			MaxDistance = Weapon().WeaponAttributes.Range() - 0.25f;
			if (MaxDistance > 4f) MaxDistance = 4f;
			MinDistance = 0.5f;
		}

		private bool FireConditionsMet()
		{
			if (!AimTimeMet()) return false;
			if (HasLineOfSight()) return true;
			ResetAimTime();
			return false;
		}

		private void TryFireAuto()
		{
			if (TryReload(TryFireAuto)) return;
			if (!FireConditionsMet()) return;
			if (!WeaponBehaviour.FireRateTargetMet()) return;
			WeaponBehaviour.StartFiring();
		}

		private void TryFireManual()
		{
			if (TryReload(TryFireManual)) return;
			if (!FireConditionsMet()) return;
			if (WeaponBehaviour.Firing)
			{
				ResetAimTime();
				_aimTime /= 2f;
				return;
			}

			WeaponBehaviour.StartFiring();
		}

		private bool TryReload(Action onReloadCallback)
		{
			if (!WeaponBehaviour.Empty()) return false;
			Reload(onReloadCallback);
			return true;
		}

		private void Reload(Action currentActionCallback)
		{
			float duration = _reloadDuration;
			WeaponBehaviour.StopFiring();
			CurrentAction = () =>
			{
				duration -= Time.deltaTime;
				if (duration > 0) return;
				WeaponBehaviour.Reload();
				ResetAimTime();
				CurrentAction = currentActionCallback;
			};
		}

		private bool AimTimeMet()
		{
			if (_aimTime    > 0f) _aimTime -= Time.deltaTime;
			return _aimTime <= 0f;
		}


		private void ResetAimTime()
		{
			_aimTime = Random.Range(0.25f, 0.5f);
			WeaponBehaviour.StopFiring();
		}

		public override Weapon Weapon()         => Enemy.Weapon;
		public override string GetDisplayName() => Enemy.Template.DisplayName;

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