using System.Collections;
using Extensions;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using SamsHelper;
using UnityEngine;

namespace Game.Gear.Weapons
{
	public sealed class WeaponBehaviour : MonoBehaviour
	{
		private Weapon           _weapon;
		private WeaponAttributes _weaponAttributes;
		private float            TimeToNextFire;
		private CharacterCombat  _origin;
		private bool             _firing;
		private int              _ammoInMagazine;
		public  bool             InvertedAccuracy;
		private int              _repeatCount;
		private float            _repeatInterval;
		private bool             _stillFiring;

		public Weapon Weapon => _weapon;
		public bool   Firing => _firing;

		public void Initialise(CharacterCombat origin)
		{
			_origin           = origin;
			_weapon           = origin.Weapon();
			_weaponAttributes = _weapon.WeaponAttributes;
			Reload();
		}

		public void Reload(int shotsNow)
		{
			_ammoInMagazine = shotsNow;
			_firing         = false;
		}

		public void SetRepeat(int count, float interval)
		{
			_repeatCount    = count;
			_repeatInterval = interval;
		}

		public void Reload() => Reload(Capacity());

		public bool FullyLoaded() => GetRemainingAmmo() == Capacity();

		public int Capacity() => _weaponAttributes.Capacity();

		public bool Empty() => GetRemainingAmmo() == 0;

		public int GetRemainingAmmo() => _ammoInMagazine;

		public bool FireRateTargetMet()
		{
			return Helper.TimeInSeconds() >= TimeToNextFire;
		}

		public bool CanFire()
		{
			bool needsTriggerPull = !_weaponAttributes.Automatic && _firing;
			return !Empty() && FireRateTargetMet() && !needsTriggerPull;
		}

		public void StartFiring()
		{
			if (_stillFiring) return;
			_firing = true;
			StartCoroutine(SecondaryFire());
		}

		private IEnumerator SecondaryFire()
		{
			_stillFiring = true;
			Fire();
			int   repeatCount = _repeatCount - 1;
			float timer       = _repeatInterval;
			for (int i = 0; i < repeatCount; ++i)
			{
				while (timer > 0f)
				{
					timer -= Time.deltaTime;
					yield return null;
				}

				Fire();
				timer = _repeatInterval;
			}

			_stillFiring = false;
		}

		public void StopFiring() => _firing = false;

		private void Fire()
		{
			if (Empty()) return;
			TimeToNextFire = 1f / _weapon.WeaponAttributes.FireRate();
			if (_origin is EnemyBehaviour) TimeToNextFire *= 2f;
			TimeToNextFire += Helper.TimeInSeconds();
			for (int i = 0; i < _weaponAttributes.Pellets(); ++i)
			{
				Shot shot = ShotManager.Create(_origin);
				_origin.ApplyShotEffects(shot);
				if (_weapon.WeaponAttributes.GetWeaponClass() == WeaponClassType.Voidwalker) shot.Attributes().Piercing = true;
				shot.Fire();
			}

			if (_origin is PlayerCombat) PlayerCombat.Instance.Shake(_weapon.WeaponAttributes.DPS());
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