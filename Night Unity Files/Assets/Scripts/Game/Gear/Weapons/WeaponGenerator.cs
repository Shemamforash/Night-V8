using System;
using System.Collections.Generic;
using System.Xml;
using Game.Combat.Weapons;
using Game.World.WorldEvents;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class WeaponGenerator : MonoBehaviour
    {
        private static readonly Dictionary<WeaponType, WeaponClass> WeaponDictionary =
            new Dictionary<WeaponType, WeaponClass>();

        public void Awake()
        {
            LoadBaseWeapons();
        }

        private void LoadBaseWeapons()
        {
            TextAsset weaponFile = Resources.Load<TextAsset>("WeaponClasses");
            XmlDocument weaponXml = new XmlDocument();
            weaponXml.LoadXml(weaponFile.text);
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                XmlNode classNode = weaponXml.SelectSingleNode("//Class[@name='" + type + "']");
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
                    string subtypeName = subtypeNode.Attributes?["name"].Value;
                    int noPellets = int.Parse(subtypeNode.SelectSingleNode("Pellets").InnerText);
                    int capacity = int.Parse(subtypeNode.SelectSingleNode("Capacity").InnerText);
                    AttributesModifier modifier = new AttributesModifier();
                    ReadModifierValue(AttributeType.Damage, subtypeNode, modifier);
                    ReadModifierValue(AttributeType.Accuracy, subtypeNode, modifier);
                    ReadModifierValue(AttributeType.FireRate, subtypeNode, modifier);
                    ReadModifierValue(AttributeType.Handling, subtypeNode, modifier);
                    ReadModifierValue(AttributeType.ReloadSpeed, subtypeNode, modifier);
                    ReadModifierValue(AttributeType.CriticalChance, subtypeNode, modifier);
                    baseWeapon.AddSubtype(new WeaponSubClass(subtypeName, capacity, noPellets, modifier));
                }
            }
        }

        private void ReadModifierValue(AttributeType attributeType, XmlNode node, AttributesModifier modifier)
        {
            modifier.AddModifier(attributeType, float.Parse(node.SelectSingleNode(attributeType.ToString()).InnerText));
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
            WeaponSubClass weaponSubClass = weaponClass.GetSubtype();

            if (weaponClass.CanBeManual)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f)
                {
                    automatic = false;
                }
            }

            string weaponName = GenerateName(weaponClass, weaponSubClass);
            WorldEventManager.GenerateEvent(new WeaponFindEvent(weaponName));
            return new Weapon(weaponClass, weaponSubClass, automatic, 10f, 20);
        }

        private static string GenerateName(WeaponClass weaponClass, WeaponSubClass weaponSubClass)
        {
            string name = weaponClass.Type + " " + weaponSubClass.Name;
            return name;
        }
    }
}