using Articy.Night;
using JetBrains.Annotations;
using UnityEngine;

namespace Game.Combat
{
    public class Weapon
    {
        public readonly float Damage, Accuracy, ReloadSpeed, CriticalChance, Handling, FireRate, Capacity;
        private WeaponBase _baseWeapon;
        private bool _automatic;
        
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
            Capacity = baseWeapon.GetAttributeValue(WeaponBase.Attributes.Capacity);
            
            if (!automatic)
            {
                Damage *= 2;
                Capacity = Mathf.Ceil(Capacity / 2);
                Accuracy *= 1.5f;
                ReloadSpeed /= 2f;
            }
        }

        public string GetName()
        {
            string automaticString = _automatic ? "Automatic" : "Manual";
            return _baseWeapon.Rarity + " " + _baseWeapon.Suffix + "(" + automaticString + ")";
        }
    }
}