using System.Collections.Generic;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class BaseWeaponBehaviour : MonoBehaviour
    {
        protected Weapon Weapon;
        protected WeaponAttributes WeaponAttributes;
        protected int AmmoInMagazine;
        protected float TimeToNextFire;
        protected CharacterCombat Origin;
        protected bool _fired;

        public void Initialise(Weapon weapon)
        {
            Weapon = weapon;
            WeaponAttributes = Weapon.WeaponAttributes;
            Reload();
        }

        public void Reload()
        {
            AmmoInMagazine = (int) WeaponAttributes.Val(AttributeType.Capacity);
            _fired = false;
        }

        public bool FullyLoaded() => GetRemainingAmmo() == (int) WeaponAttributes.Val(AttributeType.Capacity);

        public int Capacity() => (int) WeaponAttributes.Val(AttributeType.Capacity);

        public bool Empty() => GetRemainingAmmo() == 0;

        public int GetRemainingAmmo() => AmmoInMagazine;

        protected bool FireRateTargetMet()
        {
            return Helper.TimeInSeconds() >= TimeToNextFire;
        }

        public bool CanFire()
        {
            bool needsTriggerPull = !WeaponAttributes.Automatic && _fired;
            return !Empty() && FireRateTargetMet() && !needsTriggerPull;
        }

        public virtual void StartFiring(CharacterCombat origin)
        {
            Origin = origin;
            _fired = true;
        }

        public virtual void EndFiring()
        {
            _fired = false;
        }

        public bool CanReload()
        {
            return !_fired || Empty();
        }

        protected void Fire(CharacterCombat origin)
        {
            if (Empty()) return;
            TimeToNextFire = Helper.TimeInSeconds() + 1f / Weapon.GetAttributeValue(AttributeType.FireRate);
            //todo play sound GunFire.Fire(_weaponAttributes.WeaponType, distance);
            for (int i = 0; i < WeaponAttributes.Val(AttributeType.Pellets); ++i)
            {
                Shot shot = Shot.Create(origin);
                origin.ApplyShotEffects(shot);
                shot.Fire();
            }
            ConsumeAmmo(1);
            if (origin is PlayerCombat)
            {
                PlayerCombat.Instance.MuzzleFlashOpacity = 0.2f;
                UIMagazineController.UpdateMagazineUi();
            }
        }

        private void ConsumeAmmo(int amount = 0)
        {
            WeaponAttributes.DecreaseDurability();
            AmmoInMagazine -= amount;
            if (AmmoInMagazine < 0) throw new Exceptions.MoreAmmoConsumedThanAvailableException();
        }
    }
}