﻿using System.Collections.Generic;
using System.Xml;
using Game.Combat.Enemies;
using SamsHelper.Libraries;
using SamsHelper.Persistence;

namespace Game.Combat.Generation
{
    public class CombatScenario : IPersistenceTemplate
    {
        private const int MaxEncounterSize = 10;

        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private bool _finished;

        public void Load(XmlNode doc, PersistenceType saveType)
        {
//            throw new System.NotImplementedException();
        }

        public XmlNode Save(XmlNode doc, PersistenceType type)
        {
//            throw new System.NotImplementedException();
            return doc;
        }

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
    }
}