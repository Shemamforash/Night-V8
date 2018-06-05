using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Exploration.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
        private static readonly Dictionary<WeaponType, List<WeaponClass>> WeaponClasses = new Dictionary<WeaponType, List<WeaponClass>>();
        private static bool _readWeapons;


        public static WeaponClass GetWeaponClassWithType(WeaponType type)
        {
            LoadBaseWeapons();
            List<WeaponClass> validTypes = WeaponClasses[type];
            return validTypes[Random.Range(0, validTypes.Count)];
        }

        public static Weapon GenerateWeapon(ItemQuality quality, WeaponType type)
        {
            return GenerateWeapon(quality, new List<WeaponType> {type});
        }

        public static Weapon GenerateWeapon(ItemQuality quality, List<WeaponType> weaponsWanted = null)
        {
            LoadBaseWeapons();
            WeaponType weaponType;
            if (weaponsWanted != null)
            {
                weaponType = weaponsWanted.Count != 0 ? weaponsWanted[Random.Range(0, weaponsWanted.Count)] : weaponsWanted[0];
            }
            else
            {
                Array types = Enum.GetValues(typeof(WeaponType));
                weaponType = (WeaponType) types.GetValue(Random.Range(0, types.Length));
            }

            WeaponClass weaponClass = GetWeaponClassWithType(weaponType);
            Weapon weapon = weaponClass.CreateWeapon(quality);
            WorldEventManager.GenerateEvent(new WeaponFindEvent(weapon.Name));
            weapon.SetName();
            return weapon;
        }

        private static void LoadBaseWeapons()
        {
            if (_readWeapons) return;
            TextAsset weaponFile = Resources.Load<TextAsset>("XML/WeaponClasses");
            XmlDocument weaponXml = new XmlDocument();
            weaponXml.LoadXml(weaponFile.text);
            XmlNode classesNode = weaponXml.SelectSingleNode("//Weapons");
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                WeaponClasses[type] = new List<WeaponClass>();
                XmlNode classNode = classesNode.SelectSingleNode("Class[@name='" + type + "']");
                int ammoCost = int.Parse(classNode.Attributes["ammoCost"].Value);

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
                    int pellets = int.Parse(subtypeNode.SelectSingleNode("Pellets").InnerText);
                    WeaponClass weapon = new WeaponClass(type, name, automatic, ammoCost, damage, fireRate, reloadSpeed, accuracy, handling, capacity, pellets);
                    WeaponClasses[type].Add(weapon);
                }
            }

            _readWeapons = true;
        }
    }
}