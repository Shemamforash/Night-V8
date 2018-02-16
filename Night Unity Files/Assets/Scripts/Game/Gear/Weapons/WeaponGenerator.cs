using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Gear.Armour;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
        private static readonly List<GearModifier> GeneralModifiers = new List<GearModifier>();
        private static readonly Dictionary<WeaponType, List<WeaponClass>> WeaponClasses = new Dictionary<WeaponType, List<WeaponClass>>();

        static WeaponGenerator()
        {
            LoadBaseWeapons();
            GearReader.LoadGear();
//            WeaponGenerationTester.Test();
        }

        public static WeaponClass GetWeaponClassWithType(WeaponType type)
        {
            List<WeaponClass> validTypes = WeaponClasses[type];
            return validTypes[Random.Range(0, validTypes.Count)];
        }

        public static Weapon GenerateWeapon(WeaponType type, int durability = -1)
        {
            return GenerateWeapon(new List<WeaponType> {type}, durability);
        }

        public static Weapon GenerateWeapon(List<WeaponType> weaponsWanted = null, int durability = -1)
        {
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
            Weapon weapon = weaponClass.CreateWeapon(durability);
            WorldEventManager.GenerateEvent(new WeaponFindEvent(weapon.Name));
            weapon.SetName();
            return weapon;
        }

        private static void LoadBaseWeapons()
        {
            TextAsset weaponFile = Resources.Load<TextAsset>("XML/WeaponClasses");
            XmlDocument weaponXml = new XmlDocument();
            weaponXml.LoadXml(weaponFile.text);
            XmlNode classesNode = weaponXml.SelectSingleNode("//Classes");
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
                    LoadModifierValues(subtypeNode, weapon);
                    WeaponClasses[type].Add(weapon);
                }
            }

            XmlNode modifiersNode = weaponXml.SelectSingleNode("//Modifiers");
            foreach (XmlNode modifierNode in modifiersNode.SelectNodes("Modifier"))
            {
                GeneralModifiers.Add(CreateModifier(modifierNode));
            }
        }

        private static GearModifier CreateModifier(XmlNode modifierNode)
        {
            string modifierName = modifierNode.Attributes?["name"].Value;
            GearModifier modifier = new GearModifier(modifierName);
            LoadModifierValues(modifierNode, modifier);
            return modifier;
        }

        private static void LoadModifierValues(XmlNode parent, GearModifier gearModifier)
        {
            foreach (XmlNode subNode in parent.ChildNodes)
            {
                string attributeName = subNode.Name;
                AttributeType attributeType = StringToAttributeType(attributeName);
                string modifierType = subNode.InnerText.Substring(0, 1);
                string modifierValue = subNode.InnerText.Substring(1);
                float value = float.Parse(modifierValue);
                AttributeModifier attributeModifier = new AttributeModifier {AttributeType = attributeType};
                if (modifierType == "+")
                {
                    attributeModifier.SetSummative(value);
                }
                else
                {
                    attributeModifier.SetMultiplicative(value);
                }

                gearModifier.AddAttributeModifier(attributeModifier);
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