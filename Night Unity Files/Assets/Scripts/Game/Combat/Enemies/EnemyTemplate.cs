﻿using System;
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
        public readonly int Health, Speed, Value;
        public readonly string DropResource, Species;
        public readonly bool HasWeapon, HasGear;

        private EnemyTemplate(XmlNode enemyNode)
        {
            EnemyType = StringToType(enemyNode.StringFromNode("Name"));
            Health = enemyNode.IntFromNode("Health");
            Speed = enemyNode.IntFromNode("Speed");
            Value = enemyNode.IntFromNode("Value");
            HasWeapon = enemyNode.BoolFromNode("HasWeapon");
            Species = enemyNode.StringFromNode("Species");
            HasGear = enemyNode.BoolFromNode("HasGear");
            EnemyTemplates[EnemyType] = this;
            DropResource = enemyNode.StringFromNode("Drops");
        }

        public static List<EnemyTemplate> GetEnemyTypes()
        {
            LoadTemplates();
            return EnemyTemplates.Values.ToList();
        }

        public static EnemyType StringToType(string typeName)
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
            foreach (XmlNode enemyNode in Helper.GetNodesWithName(root,"Enemy"))
                new EnemyTemplate(enemyNode);
            _loaded = true;
        }

        public static EnemyTemplate GetEnemyTemplate(EnemyType enemyType)
        {
            LoadTemplates();
            if (!EnemyTemplates.ContainsKey(enemyType)) throw new Exceptions.EnemyTypeDoesNotExistException(enemyType.ToString());

            return EnemyTemplates[enemyType];
        }

        public Enemy Create()
        {
            return new Enemy(this);
        }
    }
}