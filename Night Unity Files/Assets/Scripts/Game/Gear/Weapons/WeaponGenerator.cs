using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
        private static readonly Dictionary<WeaponType, WeaponClass> WeaponDictionary =
            new Dictionary<WeaponType, WeaponClass>();

        private static readonly List<GearModifier> GeneralModifiers = new List<GearModifier>();
        
        static WeaponGenerator()
        {
            LoadBaseWeapons();
            GearReader.LoadGear();
        }
        
        public static Weapon GenerateWeapon(WeaponType type, bool manualOnly = false)
        {
            return GenerateWeapon(new List<WeaponType> {type}, manualOnly);
        }

        public static Weapon GenerateWeapon(List<WeaponType> weaponsWanted = null, bool manualOnly = false)
        {
            bool automatic = true;
            WeaponType weaponType;
            if (weaponsWanted != null && weaponsWanted.Count != 0)
            {
                weaponType = weaponsWanted[UnityEngine.Random.Range(0, weaponsWanted.Count)];
            }
            else
            {
                Array types = Enum.GetValues(typeof(WeaponType));
                weaponType = (WeaponType) types.GetValue(UnityEngine.Random.Range(0, types.Length));
            }
#if UNITY_EDITOR
            if (manualOnly)
            {
                weaponType = WeaponType.Rifle;
                automatic = false;
            }
#endif
            WeaponClass weaponClass = WeaponDictionary[weaponType];
            GearModifier subClass = weaponClass.GetSubtype();

            if (weaponClass.CanBeManual)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f)
                {
                    automatic = false;
                }
            }

            GearModifier modifier = GeneralModifiers[UnityEngine.Random.Range(0, GeneralModifiers.Count)];
            return new Weapon(weaponClass, subClass, modifier, automatic, 10f, UnityEngine.Random.Range(0, 20));
        }
        
        private static void LoadBaseWeapons()
        {
            TextAsset weaponFile = Resources.Load<TextAsset>("WeaponClasses");
            XmlDocument weaponXml = new XmlDocument();
            weaponXml.LoadXml(weaponFile.text);
            XmlNode classesNode = weaponXml.SelectSingleNode("//Classes");
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                XmlNode classNode = classesNode.SelectSingleNode("Class[@name='" + type + "']");
                bool canBeManual = classNode.Attributes["manualAllowed"].Value == "True";

                WeaponClass baseWeapon = new WeaponClass(type, canBeManual);
                WeaponDictionary[type] = baseWeapon;
                
                LoadModifierValues(classNode.SelectSingleNode("BaseStats"), baseWeapon);

                foreach (XmlNode subtypeNode in classNode.SelectNodes("Subtype"))
                {
                    baseWeapon.AddSubtype(CreateModifier(subtypeNode));
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
                    attributeModifier.SetMultiplicative(value - 1);
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