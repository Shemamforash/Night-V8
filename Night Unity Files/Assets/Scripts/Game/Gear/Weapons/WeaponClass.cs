﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public class WeaponClass
    {
        public readonly bool Automatic;
        public readonly WeaponClassType Name;
        public readonly WeaponType Type;
        public readonly string Description, FireType, FireMode;
        public readonly int Pellets, Capacity, Recoil, Damage;
        public readonly float ReloadSpeed, FireRate, Accuracy;
        private static readonly List<WeaponClassType> _weaponClassTypes = new List<WeaponClassType>();
        private static readonly List<WeaponClass> _weaponClasses = new List<WeaponClass>();

        public WeaponClass(XmlNode subtypeNode, WeaponType type)
        {
            Type = type;
            Automatic = subtypeNode.NodeAttributeValue("automatic") == "True";
            Name = NameToClassType(subtypeNode.Attributes["name"].Value);
            Damage = subtypeNode.IntFromNode("Damage");
            FireRate = subtypeNode.FloatFromNode("FireRate");
            ReloadSpeed = subtypeNode.FloatFromNode("ReloadSpeed");
            Accuracy = subtypeNode.FloatFromNode("Accuracy") / 100f;
            Recoil = subtypeNode.IntFromNode("Recoil");
            Capacity = subtypeNode.IntFromNode("Capacity");
            FireType = subtypeNode.StringFromNode("FireType");
            FireMode = subtypeNode.StringFromNode("FireMode");
            Description = FireType + " - " + Type;
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

        public static WeaponClass IntToWeaponClass(int weaponClassString)
        {
            WeaponGenerator.LoadBaseWeapons();
            return _weaponClasses.First(w => (int) w.Name == weaponClassString);
        }

        public static WeaponClass GetRandomClass()
        {
            WeaponGenerator.LoadBaseWeapons();
            return _weaponClasses.RandomElement();
        }
    }
}