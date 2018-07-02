using System;
using System.Collections.Generic;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public class WeaponClass
    {
        public readonly bool Automatic;
        public readonly WeaponClassType Name;
        public readonly WeaponType Type;
        public readonly int Pellets, Capacity, Handling, Accuracy, Damage;
        public readonly float ReloadSpeed, FireRate;
        private static readonly List<WeaponClassType> _weaponClassTypes = new List<WeaponClassType>();
        private static readonly List<WeaponClass> _weaponClasses = new List<WeaponClass>();

        public WeaponClass(WeaponType type, string name, bool automatic, int damage, float fireRate, float reloadSpeed, int accuracy, int handling, int capacity)
        {
            Type = type;
            Automatic = automatic;
            Name = NameToClassType(name);
            Damage = damage;
            FireRate = fireRate;
            ReloadSpeed = reloadSpeed;
            Accuracy = accuracy;
            Handling = handling;
            Capacity = capacity;
            Pellets = type == WeaponType.Shotgun ? 10 : 1;
            _weaponClasses.Add(this);
        }

        private static WeaponClassType NameToClassType(string name)
        {
            if (_weaponClassTypes.Count == 0)
            {
                foreach (WeaponClassType classType in Enum.GetValues(typeof(WeaponClassType))) _weaponClassTypes.Add(classType);
            }
            foreach (WeaponClassType classType in _weaponClassTypes)
            {
                if (classType.ToString() == name)
                {
                    return classType;
                }
            }

            throw new ArgumentOutOfRangeException("Unknown class type: '" + name + "'");
        }

        public static WeaponClass GetRandomClass()
        {
            return Helper.RandomInList(_weaponClasses);
        }
    }
}