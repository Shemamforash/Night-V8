using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Gear.Weapons;
using SamsHelper;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class EnemyTemplate
    {
        private static bool _loaded;
        private static readonly Dictionary<EnemyType, EnemyTemplate> EnemyTemplates = new Dictionary<EnemyType, EnemyTemplate>();
        public readonly List<WeaponType> AllowedWeaponTypes = new List<WeaponType>();
        public readonly EnemyType EnemyType;
        public readonly int Health;
        public readonly int Speed;
        public readonly int Value;

        private EnemyTemplate(EnemyType type, int health, int speed, int value, string[] allowedWeaponTypes)
        {
            EnemyType = type;
            Health = health;
            Speed = speed;
            Value = value;
            foreach (WeaponType weaponType in Enum.GetValues(typeof(WeaponType)))
                if (allowedWeaponTypes.Contains(weaponType.ToString()))
                    AllowedWeaponTypes.Add(weaponType);
        }

        public static List<EnemyTemplate> GetEnemyTypes()
        {
            LoadTemplates();
            return EnemyTemplates.Values.ToList();
        }

        private static EnemyType NameToType(string typeName)
        {
            foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType)))
                if (enemyType.ToString() == typeName)
                    return enemyType;
            throw new Exceptions.EnemyTypeDoesNotExistException(typeName);
        }

        private static void LoadTemplates()
        {
            if (_loaded) return;
            TextAsset enemyFile = Resources.Load<TextAsset>("XML/Enemies");
            XmlDocument enemyXml = new XmlDocument();
            enemyXml.LoadXml(enemyFile.text);
            XmlNode root = enemyXml.SelectSingleNode("Enemies");
            foreach (XmlNode accessoryNode in root.SelectNodes("Enemy"))
            {
                string name = accessoryNode.SelectSingleNode("Name").InnerText;
                EnemyType type = NameToType(name);
                int health = int.Parse(accessoryNode.SelectSingleNode("Health").InnerText);
                int speed = int.Parse(accessoryNode.SelectSingleNode("Speed").InnerText);
                int value = int.Parse(accessoryNode.SelectSingleNode("Value").InnerText);
                string[] allowedWeaponTypes = accessoryNode.SelectSingleNode("WeaponTypes").InnerText.Replace(" ", "").Split(',');
                EnemyTemplate t = new EnemyTemplate(type, health, speed, value, allowedWeaponTypes);
                EnemyTemplates[type] = t;
            }

            _loaded = true;
        }

        public static EnemyTemplate GetEnemyTemplate(EnemyType enemyType)
        {
            LoadTemplates();
            if (!EnemyTemplates.ContainsKey(enemyType)) throw new Exceptions.EnemyTypeDoesNotExistException(enemyType.ToString());

            return EnemyTemplates[enemyType];
        }
    }
}