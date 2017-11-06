using System;
using System.Collections.Generic;
using System.Xml;
using Game.Gear.Armour;
using Game.World.WorldEvents;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
        private static readonly Dictionary<WeaponType, WeaponClass> WeaponDictionary =
            new Dictionary<WeaponType, WeaponClass>();

        private static readonly List<WeaponModifier> _generalModifiers = new List<WeaponModifier>();

        static WeaponGenerator()
        {
            GearReader.LoadGear();
            LoadBaseWeapons();
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

                XmlNode baseStats = classNode.SelectSingleNode("BaseStats");
                baseWeapon.Damage = int.Parse(baseStats.SelectSingleNode("Damage").InnerText);
                baseWeapon.Accuracy = int.Parse(baseStats.SelectSingleNode("Accuracy").InnerText);
                baseWeapon.FireRate = float.Parse(baseStats.SelectSingleNode("FireRate").InnerText);
                baseWeapon.Handling = int.Parse(baseStats.SelectSingleNode("Handling").InnerText);
                baseWeapon.ReloadSpeed = float.Parse(baseStats.SelectSingleNode("ReloadSpeed").InnerText);
                baseWeapon.CriticalChance = int.Parse(baseStats.SelectSingleNode("CriticalChance").InnerText);

                foreach (XmlNode subtypeNode in classNode.SelectNodes("Subtype"))
                {
                    baseWeapon.AddSubtype(CreateModifier(subtypeNode, true));
                }
            }
            XmlNode modifiersNode = weaponXml.SelectSingleNode("//Modifiers");
            foreach (XmlNode modifierNode in modifiersNode.SelectNodes("Modifier"))
            {
                _generalModifiers.Add(CreateModifier(modifierNode));
            }
        }

        private static WeaponModifier CreateModifier(XmlNode modifierNode, bool isSubclass = false)
        {
            string modifierName = modifierNode.Attributes?["name"].Value;
            int noPellets = int.Parse(modifierNode.SelectSingleNode("Pellets").InnerText);
            float capacity = float.Parse(modifierNode.SelectSingleNode("Capacity").InnerText);
            WeaponModifier modifier;
            if (isSubclass)
            {
                modifier = new WeaponModifier(modifierName, (int) capacity, noPellets);
            }
            else
            {
                modifier = new WeaponModifier(modifierName, 0, noPellets, capacity);
            }
            ReadModifierValue(AttributeType.Damage, modifierNode, modifier);
            ReadModifierValue(AttributeType.Accuracy, modifierNode, modifier);
            ReadModifierValue(AttributeType.FireRate, modifierNode, modifier);
            ReadModifierValue(AttributeType.Handling, modifierNode, modifier);
            ReadModifierValue(AttributeType.ReloadSpeed, modifierNode, modifier);
            ReadModifierValue(AttributeType.CriticalChance, modifierNode, modifier);
            return modifier;
        }

        private static void ReadModifierValue(AttributeType attributeType, XmlNode node, AttributesModifier modifier, bool summative = false)
        {
            float modifierValue = float.Parse(node.SelectSingleNode(attributeType.ToString()).InnerText);
            if (!summative) modifierValue -= 1;
            modifier.AddModifier(attributeType, modifierValue, summative);
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
            WeaponModifier subClass = weaponClass.GetSubtype();

            if (weaponClass.CanBeManual)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f)
                {
                    automatic = false;
                }
            }

            WeaponModifier modifier = _generalModifiers[UnityEngine.Random.Range(0, _generalModifiers.Count)];
            return new Weapon(weaponClass, subClass, modifier, automatic, 10f, UnityEngine.Random.Range(0, 20));
        }
    }
}