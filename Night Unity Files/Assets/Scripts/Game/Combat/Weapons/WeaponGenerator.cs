using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Game.Combat.Weapons
{
    public class WeaponGenerator : MonoBehaviour
    {
        private string _baseWeaponInfoPath;

        private static readonly Dictionary<WeaponType, Dictionary<WeaponRarity, WeaponBase>> WeaponDictionary =
            new Dictionary<WeaponType, Dictionary<WeaponRarity, WeaponBase>>();

        public enum WeaponRarity
        {
            Rusty,
            Tarnished,
            Shiny,
            Gleaming,
            Radiant
        }

        public enum WeaponType
        {
            Pistol,
            Rifle,
            Shotgun,
            SMG,
            LMG,
            Explosive
        }

        public void Awake()
        {
            _baseWeaponInfoPath = Application.dataPath + "/Resources/WeaponClasses.xml";
            LoadBaseWeapons();
        }

        private void LoadBaseWeapons()
        {
            XmlDocument weaponXml = new XmlDocument();
            weaponXml.Load(_baseWeaponInfoPath);
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                Dictionary<WeaponRarity, WeaponBase> rarityWeaponDictionary =
                    new Dictionary<WeaponRarity, WeaponBase>();
                WeaponDictionary[type] = rarityWeaponDictionary;
                XmlNode classNode = weaponXml.SelectSingleNode("//Class[@name='" + type + "']");
                foreach (WeaponRarity rarity in Enum.GetValues(typeof(WeaponRarity)))
                {
                    XmlNode rarityNode = classNode.SelectSingleNode(rarity.ToString());
                    string suffix = rarityNode.SelectSingleNode("Suffix").InnerText;
                    XmlNode statsNode = rarityNode.SelectSingleNode("BaseStats");


                    WeaponBase baseWeapon = new WeaponBase(type, rarity, suffix);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.Damage, statsNode);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.Accuracy, statsNode);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.FireRate, statsNode);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.Handling, statsNode);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.ReloadSpeed, statsNode);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.Capacity, statsNode);
                    ReadMinMaxValuesFromLine(baseWeapon, WeaponBase.Attributes.CriticalChance, statsNode);

                    rarityWeaponDictionary[rarity] = baseWeapon;
                }
            }
        }

        private void ReadMinMaxValuesFromLine(WeaponBase weapon,
            WeaponBase.Attributes attribute, XmlNode attributeNode)
        {
            string[] text = attributeNode.SelectSingleNode(attribute.ToString()).InnerText.Split('-');
            float min = 0, max = 10;
            if (text.Length == 2)
            {
                if (text[0] != "")
                {
                    min = float.Parse(text[0]);
                }
                if (text[1] != "")
                {
                    max = float.Parse(text[1]);
                }
                else
                {
                    max = min + 10;
                }
            }
            weapon.SetAttribute(attribute, min, max);
        }

        public static Weapon GenerateWeapon()
        {
            bool automatic = true;
            Array types = Enum.GetValues(typeof(WeaponType));
            WeaponType weaponType = (WeaponType) types.GetValue(UnityEngine.Random.Range(0, types.Length));
#if UNITY_EDITOR
            bool manualRequired = true;
            if (manualRequired)
            {
                weaponType = WeaponType.Rifle;
                automatic = false;
            }
#endif
            Array rarities = Enum.GetValues(typeof(WeaponRarity));
            WeaponRarity weaponRarity =
                (WeaponRarity) rarities.GetValue(UnityEngine.Random.Range(0, rarities.Length));
            WeaponBase baseWeapon = WeaponDictionary[weaponType][weaponRarity];

            if (weaponType == WeaponType.Pistol || weaponType == WeaponType.Rifle)
            {
                if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.6f)
                {
                    automatic = false;
                }
            }

            return new Weapon(baseWeapon, automatic);
        }
    }
}