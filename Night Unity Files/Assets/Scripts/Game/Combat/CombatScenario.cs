using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Humans;
using SamsHelper;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Combat
{
    public class CombatScenario : IPersistenceTemplate
    {
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private bool _finished;
        private const int MaxEncounterSize = 6;

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

        private static float RandomPosition()
        {
            return Random.Range(40f, 60f);
        }
        
        private static void AddEnemy(EnemyType enemyType, CombatScenario scenario)
        {
            switch (enemyType)
            {
                case EnemyType.Sentinel:
                    scenario.AddEnemy(new Enemy(EnemyType.Sentinel));
                    break;
                case EnemyType.Martyr:
                    scenario.AddEnemy(new Enemy(EnemyType.Martyr));
                    break;
                case EnemyType.Brawler:
                    scenario.AddEnemy(new Enemy(EnemyType.Brawler));
                    break;
                case EnemyType.Sniper:
                    scenario.AddEnemy(new Enemy(EnemyType.Sniper));
                    break;
                case EnemyType.Mountain:
                    scenario.AddEnemy(new Enemy(EnemyType.Mountain));
                    break;
                case EnemyType.Witch:
                    scenario.AddEnemy(new Enemy(EnemyType.Witch));
                    break;
                case EnemyType.Medic:
                    scenario.AddEnemy(new Enemy(EnemyType.Medic));
                    break;
                case EnemyType.Warlord:
                    scenario.AddEnemy(new Enemy(EnemyType.Warlord));
                    break;
            }
        }
        
        public bool ReachedMaxEncounterSize()
        {
            return _enemies.Count == MaxEncounterSize;
        }

        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();
        
        public static CombatScenario Generate(int difficulty)
        {
            CombatScenario scenario = new CombatScenario();
            while (difficulty > 0 && !scenario.ReachedMaxEncounterSize())
            {
                Helper.Shuffle(ref _enemyTypes);
                foreach(EnemyTemplate t in _enemyTypes)
                {
                    if (difficulty < t.Value) continue;
                    AddEnemy(t.EnemyType, scenario);
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