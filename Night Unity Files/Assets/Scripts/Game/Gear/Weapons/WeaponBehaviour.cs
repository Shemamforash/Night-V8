using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using Extensions;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
	public class WeaponBehaviour : MonoBehaviour
	{
		private int              _ammoInMagazine;
		private CharacterCombat  _origin;
		private Weapon           _weapon;
		private float            TimeToNextFire;


		private void Start()
		{
			_origin = GetComponent<CharacterCombat>();
			_weapon = _origin.Weapon();
			Reload();
		}

		public void Reload(int shotsNow) => _ammoInMagazine = shotsNow;

		public void Reload()
		{
			Reload((int) _weapon.Val(AttributeType.Capacity));
		}

		public bool FullyLoaded() => GetRemainingAmmo() == (int) _weapon.Val(AttributeType.Capacity);

		public int Capacity() => (int) _weapon.Val(AttributeType.Capacity);

		public bool Empty() => GetRemainingAmmo() == 0;

		public int GetRemainingAmmo() => _ammoInMagazine;

		private bool FireRateTargetMet() => Helper.TimeInSeconds() >= TimeToNextFire;

		public bool CanFire() => !Empty() && FireRateTargetMet();

		public void StartFiring()
		{
			Fire();
		}

		public void StopFiring()
		{
		}

		private void Fire()
		{
			if (Empty()) return;
			TimeToNextFire = Helper.TimeInSeconds() + 1f / _weapon.Val(AttributeType.FireRate);
			if (_origin is EnemyBehaviour) TimeToNextFire *= 2f;
			for (int i = 0; i < _weapon.Val(AttributeType.Pellets); ++i)
			{
				Shot shot = ShotManager.Create(_origin);
				_origin.ApplyShotEffects(shot);
				shot.Fire();
			}

			if (_origin is PlayerCombat) PlayerCombat.Instance.Shake(_weapon.DPS());
			_origin.WeaponAudio.Fire(_weapon);
			ConsumeAmmo(1);
			if (!(_origin is PlayerCombat)) return;
			if (Empty() && CombatManager.Instance().GetEnemiesInRange(transform.position, 5).Count > 0) PlayerCombat.Instance.Player.BrandManager.IncreasePerfectReloadCount();
			PlayerCombat.Instance.MuzzleFlashOpacity = 0.2f;
			UIMagazineController.UpdateMagazineUi();
		}

		private void ConsumeAmmo(int amount = -1)
		{
			if (amount < 0) amount = _ammoInMagazine;
			_ammoInMagazine -= amount;
			if (_ammoInMagazine < 0) throw new Exceptions.MoreAmmoConsumedThanAvailableException();
		}

		public void IncreaseAmmo(int amount)
		{
			_ammoInMagazine += amount;
			if (_ammoInMagazine > Capacity()) _ammoInMagazine = Capacity();
			if (!(_origin is PlayerCombat)) return;
			UIMagazineController.UpdateMagazineUi();
		}
	}
}