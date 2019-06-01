using Extensions;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class BaseWeaponBehaviour : MonoBehaviour
    {
        private Weapon _weapon;
        private WeaponAttributes _weaponAttributes;
        private float TimeToNextFire;
        private CharacterCombat _origin;
        private bool _fired;
        private int _ammoInMagazine;

        public void Initialise(CharacterCombat origin)
        {
            _origin = origin;
            _weapon = origin.Weapon();
            _weaponAttributes = _weapon.WeaponAttributes;
            Reload();
        }

        public void Reload(int shotsNow)
        {
            _ammoInMagazine = shotsNow;
            _fired = false;
        }

        public void Reload()
        {
            Reload((int) _weaponAttributes.Val(AttributeType.Capacity));
        }

        public bool FullyLoaded() => GetRemainingAmmo() == (int) _weaponAttributes.Val(AttributeType.Capacity);

        public int Capacity() => (int) _weaponAttributes.Val(AttributeType.Capacity);

        public bool Empty() => GetRemainingAmmo() == 0;

        public int GetRemainingAmmo() => _ammoInMagazine;

        protected bool FireRateTargetMet()
        {
            return Helper.TimeInSeconds() >= TimeToNextFire;
        }

        public bool CanFire()
        {
            bool needsTriggerPull = !_weaponAttributes.Automatic && _fired;
            return !Empty() && FireRateTargetMet() && !needsTriggerPull;
        }

        public virtual void StartFiring()
        {
            _fired = true;
        }

        public void StopFiring()
        {
            _fired = false;
        }

        protected void Fire()
        {
            if (Empty()) return;
            TimeToNextFire = Helper.TimeInSeconds() + 1f / _weapon.GetAttributeValue(AttributeType.FireRate);
            if (_origin is EnemyBehaviour) TimeToNextFire *= 2f;
            for (int i = 0; i < _weaponAttributes.Val(AttributeType.Pellets); ++i)
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

        public Weapon Weapon => _weapon;

        public void ConsumeAmmo(int amount = -1)
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