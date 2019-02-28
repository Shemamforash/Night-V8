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
        private static readonly List<EnemyType> _enemyTypes = new List<EnemyType>();
        public readonly EnemyType EnemyType;
        public readonly int Health, Speed, Value, Difficulty;
        public readonly string DropResource, Species;
        public readonly bool HasWeapon;
        public readonly float DropRate;

        private EnemyTemplate(XmlNode enemyNode)
        {
            EnemyType = StringToType(enemyNode.StringFromNode("Name"));
            Health = enemyNode.IntFromNode("Health");
            Speed = enemyNode.IntFromNode("Speed");
            Value = enemyNode.IntFromNode("Value");
            Difficulty = enemyNode.IntFromNode("Difficulty");
            HasWeapon = enemyNode.BoolFromNode("HasWeapon");
            Species = enemyNode.StringFromNode("Species");
            EnemyTemplates[EnemyType] = this;
            DropResource = enemyNode.StringFromNode("Drops");
            DropRate = enemyNode.FloatFromNode("DropRate");
        }

        public static List<EnemyTemplate> GetEnemyTypes()
        {
            LoadTemplates();
            return EnemyTemplates.Values.ToList();
        }

        private static EnemyType StringToType(string typeName)
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
            foreach (XmlNode enemyNode in root.GetNodesWithName("Enemy"))
                new EnemyTemplate(enemyNode);
            _loaded = true;
        }

        private static EnemyTemplate GetEnemyTemplate(EnemyType enemyType)
        {
            LoadTemplates();
            if (!EnemyTemplates.ContainsKey(enemyType)) throw new Exceptions.EnemyTypeDoesNotExistException(enemyType.ToString());
            return EnemyTemplates[enemyType];
        }

        public static EnemyBehaviour Create(EnemyType enemyType)
        {
            return new Enemy(GetEnemyTemplate(enemyType)).GetEnemyBehaviour();
        }

        public static List<EnemyType> RandomiseEnemiesToSize(List<EnemyType> validTypes, int size)
        {
            List<EnemyType> enemyTypes = new List<EnemyType>();
            while (size > 0)
            {
                validTypes.Shuffle();
                foreach (EnemyType e in validTypes)
                {
                    int enemyValue = GetEnemyTemplate(e).Value;
                    if (enemyValue > size) continue;
                    enemyTypes.Add(e);
                    size -= enemyValue;
                    break;
                }
            }

            return enemyTypes;
        }

        public static int GetEnemyValue(EnemyType enemyType) => GetEnemyTemplate(enemyType).Value;
    }
}