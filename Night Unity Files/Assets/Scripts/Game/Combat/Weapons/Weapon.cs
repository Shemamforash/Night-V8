using Game.World;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Weapons
{
    public class Weapon : InventoryItem
    {
        public readonly float Damage, Accuracy, ReloadSpeed, CriticalChance, Handling, FireRate;
        public int Capacity;
        private readonly WeaponBase _baseWeapon;
        public readonly bool Automatic;
        private int _ammoInMagazine;
        
        public Weapon(WeaponBase baseWeapon, bool automatic, string name, float weight) : base(name, weight)
        {
            _baseWeapon = baseWeapon;
            Automatic = automatic;
            Damage = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Damage);
            Accuracy = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Accuracy);
            ReloadSpeed = baseWeapon.GetAttributeValue(WeaponBase.Attributes.ReloadSpeed);
            CriticalChance = baseWeapon.GetAttributeValue(WeaponBase.Attributes.CriticalChance);
            Handling = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Handling);
            FireRate = baseWeapon.GetAttributeValue(WeaponBase.Attributes.FireRate);
            Capacity = (int)baseWeapon.GetAttributeValue(WeaponBase.Attributes.Capacity);

            if (!automatic)
            {
                Damage *= 2;
                Capacity = (int)Mathf.Ceil(Capacity / 2f);
                Accuracy *= 1.5f;
                Mathf.Clamp(Accuracy, 0, 100);
                ReloadSpeed /= 2f;
            }
            
            Reload();
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
            _ammoInMagazine += (int)ammoAvailable;
        }

        public int GetRemainingAmmo()
        {
            return _ammoInMagazine;
        }
    }
}