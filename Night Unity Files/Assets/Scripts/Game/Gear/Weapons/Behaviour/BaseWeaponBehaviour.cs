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
        protected Weapon Weapon;
        private WeaponAttributes WeaponAttributes;
        public int AmmoInMagazine;
        protected float TimeToNextFire;
        protected CharacterCombat Origin;
        private bool _fired;

        public void Initialise(CharacterCombat origin)
        {
            Origin = origin;
            Weapon = origin.Weapon();
            WeaponAttributes = Weapon.WeaponAttributes;
            Reload();
        }

        public virtual void Reload()
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

        public virtual void StartFiring()
        {
            _fired = true;
        }

        public virtual void StopFiring()
        {
            _fired = false;
        }

        public bool CanReload()
        {
            return !_fired || Empty();
        }

        protected void Fire()
        {
            if (Empty()) return;
            TimeToNextFire = Helper.TimeInSeconds() + 1f / Weapon.GetAttributeValue(AttributeType.FireRate);
            for (int i = 0; i < WeaponAttributes.Val(AttributeType.Pellets); ++i)
            {
                Shot shot = Shot.Create(Origin);
                Origin.ApplyShotEffects(shot);
                if (this is HoldAndFire) shot.Attributes().Piercing = true;
                shot.Fire();
            }

            if (Origin is PlayerCombat) PlayerCombat.Instance.Shake(Weapon.WeaponAttributes.DPS());
            Origin.WeaponAudio.Fire(Weapon);
            ConsumeAmmo(1);
            if (!(Origin is PlayerCombat)) return;
            PlayerCombat.Instance.MuzzleFlashOpacity = 0.2f;
            UIMagazineController.UpdateMagazineUi();
        }

        public void ConsumeAmmo(int amount = -1)
        {
            float durabilityModifier = 1;
            if (amount < 0) amount = AmmoInMagazine;
            WeaponAttributes.DecreaseDurability(amount, durabilityModifier);
            AmmoInMagazine -= amount;
            if (AmmoInMagazine < 0) throw new Exceptions.MoreAmmoConsumedThanAvailableException();
        }

        public void IncreaseAmmo(int amount)
        {
            AmmoInMagazine += amount;
            if (AmmoInMagazine > Capacity()) AmmoInMagazine = Capacity();
            if (!(Origin is PlayerCombat)) return;
            UIMagazineController.UpdateMagazineUi();
        }
    }
}