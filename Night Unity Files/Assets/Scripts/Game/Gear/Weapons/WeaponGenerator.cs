using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
//        private static readonly List<GearModifier> GeneralModifiers = new List<GearModifier>();
        private static readonly Dictionary<WeaponType, List<WeaponClass>> WeaponClasses = new Dictionary<WeaponType, List<WeaponClass>>();
        private static bool _readWeapons;


        public static WeaponClass GetWeaponClassWithType(WeaponType type)
        {
            LoadBaseWeapons();
            List<WeaponClass> validTypes = WeaponClasses[type];
            return validTypes[Random.Range(0, validTypes.Count)];
        }

        public static Weapon GenerateWeapon(ItemQuality quality, WeaponType type, int durability = -1)
        {
            return GenerateWeapon(quality, new List<WeaponType> {type}, durability);
        }

        public static Weapon GenerateWeapon(ItemQuality quality, List<WeaponType> weaponsWanted = null, int durability = -1)
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
            Weapon weapon = weaponClass.CreateWeapon(quality, durability);
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
                    WeaponClass weapon = new WeaponClass(type, name, automatic, ammoCost);
                    LoadWeaponClassValues(subtypeNode, weapon);
                    WeaponClasses[type].Add(weapon);
                }
            }

            _readWeapons = true;
        }

        private static void LoadWeaponClassValues(XmlNode parent, WeaponClass weaponClass)
        {
            foreach (XmlNode subNode in parent.ChildNodes)
            {
                string attributeName = subNode.Name;
                AttributeType attributeType = StringToAttributeType(attributeName);
                string modifierType = subNode.InnerText.Substring(0, 1);
                string modifierValue = subNode.InnerText.Substring(1);
                float value = float.Parse(modifierValue);
                AttributeModifier attributeModifier = new AttributeModifier (attributeType);
                if (modifierType == "+")
                {
                    attributeModifier.SetSummative(value);
                }
                else
                {
                    attributeModifier.SetMultiplicative(value);
                }

                weaponClass.AddAttributeModifier(attributeModifier);
            }
        }

        private static AttributeType StringToAttributeType(string attributeString)
        {
            foreach (AttributeType type in Enum.GetValues(typeof(AttributeType)))
            {
                if (type.ToString() == attributeString)
                {
                    return type;
                }
            }

            throw new Exception("Attribute string '" + attributeString + "' is not recognised.");
        }
    }
}