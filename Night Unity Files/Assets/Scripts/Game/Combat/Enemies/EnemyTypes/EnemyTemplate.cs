using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SamsHelper;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes
{
    public class EnemyTemplate
    {
        public readonly int Health;
        public readonly int Speed;
        public readonly int Value;
        public readonly string Name;

        private static bool _loaded;
        private static readonly Dictionary<string, EnemyTemplate> EnemyTemplates = new Dictionary<string, EnemyTemplate>();

        private EnemyTemplate(string name, int health, int speed, int value)
        {
            Name = name;
            Health = health;
            Speed = speed;
            Value = value;
        }

        public static List<EnemyTemplate> GetEnemyTypes()
        {
            LoadTemplates();
            return EnemyTemplates.Values.ToList();
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
                int health = int.Parse(accessoryNode.SelectSingleNode("Health").InnerText);
                int speed = int.Parse(accessoryNode.SelectSingleNode("Speed").InnerText);
                int value  = int.Parse(accessoryNode.SelectSingleNode("Value").InnerText);
                EnemyTemplate t = new EnemyTemplate(name, health, speed,value);
                EnemyTemplates[name] = t;
            }

            _loaded = true;
        }

        public static void CreateEnemyFromTemplate(Enemy e)
        {
            LoadTemplates();
            string enemyType = e.Name;
            if (!EnemyTemplates.ContainsKey(enemyType))
            {
                throw new Exceptions.EnemyTypeDoesNotExistException(enemyType);
            }

            e.SetTemplate(EnemyTemplates[enemyType]);
        }
    }
}