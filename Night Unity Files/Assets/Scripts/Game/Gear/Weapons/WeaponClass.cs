using System;
using System.Collections.Generic;
using System.Xml;
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

        public WeaponClass(XmlNode subtypeNode, WeaponType type)
        {
            Type = type;
            Automatic = Helper.NodeAttributeValue(subtypeNode, "automatic") == "True";
            Name = NameToClassType(subtypeNode.Attributes["name"].Value);
            Damage = Helper.IntFromNode(subtypeNode, "Damage");
            FireRate = Helper.FloatFromNode(subtypeNode, "FireRate");
            ReloadSpeed = Helper.FloatFromNode(subtypeNode, "ReloadSpeed");
            Accuracy = Helper.IntFromNode(subtypeNode, "Accuracy");
            Handling = Helper.IntFromNode(subtypeNode, "Handling");
            Capacity = Helper.IntFromNode(subtypeNode, "Capacity");
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