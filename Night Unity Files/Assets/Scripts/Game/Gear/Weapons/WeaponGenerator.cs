using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Exploration.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
        private static readonly Dictionary<WeaponType, List<WeaponClass>> WeaponClasses = new Dictionary<WeaponType, List<WeaponClass>>();
        private static bool _readWeapons;
        public static readonly List<WeaponType> WeaponTypes = new List<WeaponType>();


        public static Weapon GenerateWeapon(ItemQuality quality, WeaponType type)
        {
            LoadBaseWeapons();
            return Weapon.Generate(quality, Helper.RandomInList(WeaponClasses[type]));
        }
        
        public static Weapon GenerateWeapon(ItemQuality quality)
        {
            LoadBaseWeapons();
            Weapon weapon = Weapon.Generate(quality);
            return weapon;
        }

        private static void LoadBaseWeapons()
        {
            if (_readWeapons) return;
            XmlNode classesNode = Helper.OpenRootNode("WeaponClasses", "Weapons");
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                WeaponTypes.Add(type);
                WeaponClasses[type] = new List<WeaponClass>();
                XmlNode classNode = classesNode.SelectSingleNode("Class[@name='" + type + "']");
                foreach (XmlNode subtypeNode in classNode.SelectNodes("Subtype"))
                {
                    bool automatic = subtypeNode.Attributes["automatic"].Value == "True";
                    string name = subtypeNode.Attributes["name"].Value;
                    int damage = int.Parse(subtypeNode.SelectSingleNode("Damage").InnerText);
                    float fireRate = float.Parse(subtypeNode.SelectSingleNode("FireRate").InnerText);
                    float reloadSpeed = float.Parse(subtypeNode.SelectSingleNode("ReloadSpeed").InnerText);
                    int accuracy = int.Parse(subtypeNode.SelectSingleNode("Accuracy").InnerText);
                    int handling = int.Parse(subtypeNode.SelectSingleNode("Handling").InnerText);
                    int capacity = int.Parse(subtypeNode.SelectSingleNode("Capacity").InnerText);
                    WeaponClass weapon = new WeaponClass(type, name, automatic, damage, fireRate, reloadSpeed, accuracy, handling, capacity);
                    WeaponClasses[type].Add(weapon);
                }
            }

            _readWeapons = true;
        }
    }
}