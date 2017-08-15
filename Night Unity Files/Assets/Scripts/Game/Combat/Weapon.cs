using UnityEngine;
using World;

namespace Game.Combat
{
    public class Weapon
    {
        public readonly float Damage, Accuracy, ReloadSpeed, CriticalChance, Handling, FireRate;
        public int Capacity;
        private readonly WeaponBase _baseWeapon;
        private readonly bool _automatic;
        private int _ammoInMagazine;
        
        public Weapon(WeaponBase baseWeapon, bool automatic)
        {
            _baseWeapon = baseWeapon;
            _automatic = automatic;
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
                ReloadSpeed /= 2f;
            }
            Reload();
        }

        public string GetName()
        {
            string automaticString = _automatic ? "Automatic" : "Manual";
            return _baseWeapon.Rarity + " " + _baseWeapon.Suffix + " (" + automaticString + " " + _baseWeapon.Type + ")";
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
            float ammoAvailable = Home.ConsumeResource(Resource.ResourceType.Ammo, Capacity);
            _ammoInMagazine += (int)ammoAvailable;
        }

        public int GetRemainingAmmo()
        {
            return _ammoInMagazine;
        }
    }
}