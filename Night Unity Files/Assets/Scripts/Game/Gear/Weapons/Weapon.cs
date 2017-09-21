using System;
using Game.Combat.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Weapon : EquippableItem
    {
        public readonly float Damage, Accuracy, ReloadSpeed, CriticalChance, Handling, FireRate;
        private readonly float _dps;
        public readonly int Capacity;
        private readonly WeaponBase _baseWeapon;
        public readonly bool Automatic;
        private int _ammoInMagazine;

        public Weapon(WeaponBase baseWeapon, bool automatic, string name, float weight) : base(name, weight, GearSlot.Weapon, GameObjectType.Weapon)
        {
            _baseWeapon = baseWeapon;
            Automatic = automatic;
            Damage = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Damage);
            Accuracy = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Accuracy);
            ReloadSpeed = baseWeapon.GetAttributeValue(WeaponBase.Attributes.ReloadSpeed);
            CriticalChance = baseWeapon.GetAttributeValue(WeaponBase.Attributes.CriticalChance);
            Handling = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Handling);
            FireRate = baseWeapon.GetAttributeValue(WeaponBase.Attributes.FireRate);
            Capacity = (int) baseWeapon.GetAttributeValue(WeaponBase.Attributes.Capacity);

            if (!automatic)
            {
                Damage *= 2;
                Capacity = (int) Mathf.Ceil(Capacity / 2f);
                Accuracy *= 1.5f;
                Mathf.Clamp(Accuracy, 0, 100);
                ReloadSpeed /= 2f;
            }

            float averageShotDamage = CriticalChance / 100 * Damage * 2 + (1 - CriticalChance / 100) * Damage;
            float magazineDamage = Capacity * averageShotDamage;
            float magazineDuration = Capacity / FireRate + ReloadSpeed;
            _dps = magazineDamage / magazineDuration;
            Reload();
            SetExtendedName(Name + (Automatic ? " (A)" : ""));
        }

        public string GetItemType()
        {
            return _baseWeapon.Type.ToString();
        }

        public bool Fire()
        {
            if (_ammoInMagazine > 0)
            {
                --_ammoInMagazine;
                return true;
            }
            return false;
        }

        public void Reload()
        {
            float ammoAvailable = WorldState.Inventory().DecrementResource("Ammo", Capacity);
            _ammoInMagazine += (int) ammoAvailable;
        }

        public int GetRemainingAmmo()
        {
            return _ammoInMagazine;
        }

        public override string GetSummary()
        {
            return Helper.Round(_dps, 1) + "DPS";
        }
    }
}