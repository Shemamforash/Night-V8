﻿using System;
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
        private static readonly List<WeaponType> _weaponTypes = new List<WeaponType>();


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

        public static Weapon GenerateWeapon(ItemQuality quality, WeaponClassType type)
        {
            LoadBaseWeapons();
            WeaponClass weaponClass = null;

            foreach (KeyValuePair<WeaponType, List<WeaponClass>> keyValuePair in WeaponClasses)
            {
                foreach (WeaponClass w in keyValuePair.Value)
                {
                    if (w.Name == type)
                    {
                        weaponClass = w;
                    }
                }
            }

            Weapon weapon = weaponClass.CreateWeapon(quality);
            WorldEventManager.GenerateEvent(new WeaponFindEvent(weapon.Name));
            weapon.SetName();
            return weapon;
        }

        public static Weapon GenerateWeapon(ItemQuality quality, List<WeaponType> weaponsWanted = null)
        {
            LoadBaseWeapons();
            WeaponType weaponType;
            if (_weaponTypes.Count == 0)
            {
                foreach (WeaponType w in Enum.GetValues(typeof(WeaponType))) _weaponTypes.Add(w);
            }

            if (weaponsWanted != null)
            {
                weaponType = weaponsWanted.Count != 0 ? weaponsWanted[Random.Range(0, weaponsWanted.Count)] : weaponsWanted[0];
            }
            else
            {
                weaponType = Helper.RandomInList(_weaponTypes);
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
            XmlNode classesNode = Helper.OpenRootNode("WeaponClasses", "Weapons");
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
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