using System;
using System.Collections.Generic;
using System.Xml;
using Game.Combat.Weapons;
using Game.Gear.Armour;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponGenerator : MonoBehaviour
    {
        private static readonly Dictionary<WeaponType, WeaponClass> WeaponDictionary =
            new Dictionary<WeaponType, WeaponClass>();

        private static List<WeaponModifier> _generalModifiers = new List<WeaponModifier>();

        public void Awake()
        {
            GearReader.LoadGear();
            LoadBaseWeapons();
        }

        private void LoadBaseWeapons()
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
                SetScaleableValue(ref baseWeapon.Damage, baseStats.SelectSingleNode("Damage"));
                SetScaleableValue(ref baseWeapon.Accuracy, baseStats.SelectSingleNode("Accuracy"));
                SetScaleableValue(ref baseWeapon.FireRate, baseStats.SelectSingleNode("FireRate"));
                SetScaleableValue(ref baseWeapon.Handling, baseStats.SelectSingleNode("Handling"));
                SetScaleableValue(ref baseWeapon.ReloadSpeed, baseStats.SelectSingleNode("ReloadSpeed"));
                SetScaleableValue(ref baseWeapon.CriticalChance, baseStats.SelectSingleNode("CriticalChance"));

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

        private WeaponModifier CreateModifier(XmlNode modifierNode, bool isSubclass = false)
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

        private void ReadModifierValue(AttributeType attributeType, XmlNode node, AttributesModifier modifier, bool summative = false)
        {
            float modifierValue = float.Parse(node.SelectSingleNode(attributeType.ToString()).InnerText);
            if (!summative) modifierValue -= 1;
            modifier.AddModifier(attributeType, modifierValue, summative);
        }

        private void SetScaleableValue(ref ScaleableValue value, XmlNode attributeNode)
        {
            float xCoefficient = float.Parse(attributeNode.SelectSingleNode("XCoefficient").InnerText);
            float intercept = float.Parse(attributeNode.SelectSingleNode("Intercept").InnerText);
            value = new ScaleableValue(xCoefficient, intercept);
        }

        public static Weapon GenerateWeapon()
        {
            bool automatic = true;
            Array types = Enum.GetValues(typeof(WeaponType));
            WeaponType weaponType = (WeaponType) types.GetValue(UnityEngine.Random.Range(0, types.Length));
#if UNITY_EDITOR
//            bool manualRequired = true;
//            if (manualRequired)
//            {
//                weaponType = WeaponType.Rifle;
//                automatic = false;
//            }
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