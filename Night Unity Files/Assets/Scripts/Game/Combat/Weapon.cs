using UnityEngine;

namespace Game.Combat
{
    public class Weapon : ScriptableObject
    {
        private float _damage, _accuracy, _fireRate, _handling, _reloadTime, _criticalChance;
        private int _ammoCapacity;
        private bool _automatic;
        private WeaponClass _weaponClass;
        private WeaponModifier _primaryModifier, _secondaryModifier;

        public Weapon(float damage, float accuracy, float fireRate, float handling, float reloadTime, int ammoCapacity,
            float criticalChance, bool automatic, WeaponClass weaponClass, WeaponModifier primaryModifier,
            WeaponModifier secondaryModifier)
        {
            _damage = damage;
            _accuracy = accuracy;
            _fireRate = fireRate;
            _handling = handling;
            _reloadTime = reloadTime;
            _ammoCapacity = ammoCapacity;
            _criticalChance = criticalChance;
            _automatic = automatic;
            _weaponClass = weaponClass;
            _primaryModifier = primaryModifier;
            _secondaryModifier = secondaryModifier;
        }
    }
}