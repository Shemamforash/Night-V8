using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SamsHelper;
using SamsHelper.Libraries;

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
        public readonly bool GearAllowed;

        private EnemyTemplate(XmlNode enemyNode)
        {
            EnemyType = NameToType(enemyNode.SelectSingleNode("Name").InnerText);
            Health = int.Parse(enemyNode.SelectSingleNode("Health").InnerText);
            Speed = int.Parse(enemyNode.SelectSingleNode("Speed").InnerText);
            Value = int.Parse(enemyNode.SelectSingleNode("Value").InnerText);
            string dropString = enemyNode.SelectSingleNode("Drops").InnerText;
            EnemyTemplates[EnemyType] = this;
            if (dropString == "") return;
            string[] drops = dropString.Split(' ');
            DropCount = int.Parse(drops[0]);
            DropResource = drops[1];
            GearAllowed = DropResource == "Salt";
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
                new EnemyTemplate(enemyNode);
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