using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class EnemyTemplate
    {
        private static bool _loaded;
        private static readonly Dictionary<EnemyType, EnemyTemplate> EnemyTemplates = new Dictionary<EnemyType, EnemyTemplate>();
        public readonly EnemyType EnemyType;
        public readonly int Health;
        public readonly int Speed;
        public readonly int Value;
        public readonly string DropResource;
        public readonly int DropCount;
        private static readonly List<EnemyType> _enemyTypes = new List<EnemyType>();

        private EnemyTemplate(EnemyType type, int health, int speed, int value, string dropResource, int dropCount)
        {
            EnemyType = type;
            Health = health;
            Speed = speed;
            Value = value;
            DropResource = dropResource;
            DropCount = dropCount;
        }

        public static List<EnemyTemplate> GetEnemyTypes()
        {
            LoadTemplates();
            return EnemyTemplates.Values.ToList();
        }

        private static EnemyType NameToType(string typeName)
        {
            if (_enemyTypes.Count == 0)
            {
                foreach (EnemyType enemyType in Enum.GetValues(typeof(EnemyType))) _enemyTypes.Add(enemyType);    
            }
            foreach (EnemyType enemyType in _enemyTypes)
                if (enemyType.ToString() == typeName)
                    return enemyType;
            throw new Exceptions.EnemyTypeDoesNotExistException(typeName);
        }

        private static void LoadTemplates()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Enemies", "Enemies");
            foreach (XmlNode enemyNode in root.SelectNodes("Enemy"))
            {
                string name = enemyNode.SelectSingleNode("Name").InnerText;
                EnemyType type = NameToType(name);
                int health = int.Parse(enemyNode.SelectSingleNode("Health").InnerText);
                int speed = int.Parse(enemyNode.SelectSingleNode("Speed").InnerText);
                int value = int.Parse(enemyNode.SelectSingleNode("Value").InnerText);
                string dropString = enemyNode.SelectSingleNode("Drops").InnerText;
                int dropCount = 0;
                string dropResource = "";
                if (dropString != "")
                {
                    string[] drops = dropString.Split(' ');
                    dropCount = int.Parse(drops[0]);
                    dropResource = drops[1];
                }

                EnemyTemplate t = new EnemyTemplate(type, health, speed, value, dropResource, dropCount);
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