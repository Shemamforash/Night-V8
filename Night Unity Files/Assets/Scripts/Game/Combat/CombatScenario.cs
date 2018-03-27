using System;
using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Humans;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.Persistence;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class CombatScenario : IPersistenceTemplate
    {
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private bool _finished;
        private const int MaxEncounterSize = 10;

        public void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
        }

        public static CombatScenario GenerateSimple()
        {
            CombatScenario scenario = new CombatScenario();
            scenario.AddEnemy(new Enemy(EnemyType.Sentinel));
            scenario.AddEnemy(new Enemy(EnemyType.Witch));
            return scenario;
        }

        private static void AddEnemy(EnemyType enemyType, CombatScenario scenario, int difficulty)
        {
            Enemy newEnemy = new Enemy(enemyType);
            scenario.AddEnemy(newEnemy);
            newEnemy.GenerateArmour(difficulty);
            newEnemy.GenerateWeapon(difficulty);
        }

        public bool ReachedMaxEncounterSize()
        {
            return _enemies.Count == MaxEncounterSize;
        }

        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();

        public static CombatScenario Generate(int difficulty, int size)
        {
            CombatScenario scenario = new CombatScenario();
            AddEnemy(EnemyType.Medic, scenario, difficulty);
            AddEnemy(EnemyType.Sentinel, scenario, difficulty);
            AddEnemy(EnemyType.Sniper, scenario, difficulty);

//            if (size > MaxEncounterSize) size = MaxEncounterSize;
//            for (int i = 0; i < size; ++i)
//            {
//                Helper.Shuffle(ref _enemyTypes);
//                foreach (EnemyTemplate t in _enemyTypes)
//                {
//                    if (size < t.Value) continue;
//                    AddEnemy(t.EnemyType, scenario, difficulty);
//                    break;
//                }
//            }

            return scenario;
        }

        public static CombatScenario Generate(int difficulty)
        {
            CombatScenario scenario = new CombatScenario();
            while (difficulty > 0 && !scenario.ReachedMaxEncounterSize())
            {
                Helper.Shuffle(ref _enemyTypes);
                foreach (EnemyTemplate t in _enemyTypes)
                {
                    if (difficulty < t.Value) continue;
                    AddEnemy(t.EnemyType, scenario, difficulty);
                    if (scenario.ReachedMaxEncounterSize()) break;
                    difficulty -= t.Value;
                }
            }

            return scenario;
        }

        public List<Enemy> Enemies()
        {
            return _enemies;
        }

        public void FinishCombat()
        {
            _finished = true;
        }

        public bool IsFinished()
        {
            return _finished;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
//            throw new System.NotImplementedException();
        }

        public XmlNode Save(XmlNode doc, PersistenceType type)
        {
//            throw new System.NotImplementedException();
            return doc;
        }
    }
}