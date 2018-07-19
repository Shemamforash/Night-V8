using System;
using System.Collections.Generic;
using System.Xml;
using SamsHelper.Libraries;

namespace Game.Gear.Weapons
{
    public static class WeaponGenerator
    {
        private static readonly Dictionary<WeaponType, List<WeaponClass>> WeaponClasses = new Dictionary<WeaponType, List<WeaponClass>>();
        private static bool _readWeapons;
        private static readonly List<WeaponType> WeaponTypes = new List<WeaponType>();

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
                XmlNode classNode =Helper.GetNode(classesNode, "Class[@name='" + type + "']");
                foreach (XmlNode subtypeNode in Helper.GetNodesWithName(classNode, "Subtype"))
                    WeaponClasses[type].Add(new WeaponClass(subtypeNode, type));
            }

            _readWeapons = true;
        }
    }
}