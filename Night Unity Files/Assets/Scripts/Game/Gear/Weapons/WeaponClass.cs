using System;
using Boo.Lang;
using Game.Combat.Player;

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
        }

        private WeaponClassType NameToClassType(string name)
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

        public Weapon CreateWeapon(ItemQuality quality)
        {
            Weapon w = new Weapon(Type.ToString(), 10, quality);
            WeaponAttributes weaponAttributes = w.WeaponAttributes;
            weaponAttributes.SetClass(this);
            w.WeaponSkillOne = WeaponSkills.GetWeaponSkillOne(w);
            w.WeaponSkillTwo = WeaponSkills.GetWeaponSkillTwo(w);
            return w;
        }
    }
}