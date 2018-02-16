using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Humans;
using Game.Gear.Armour;
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

        private static Enemy AddEnemy(EnemyType enemyType, CombatScenario scenario)
        {
            Enemy newEnemy = null;
            switch (enemyType)
            {
                case EnemyType.Sentinel:
                    newEnemy = new Enemy(EnemyType.Sentinel);
                    break;
                case EnemyType.Martyr:
                    newEnemy = new Enemy(EnemyType.Martyr);
                    break;
                case EnemyType.Brawler:
                    newEnemy = new Enemy(EnemyType.Brawler);
                    break;
                case EnemyType.Sniper:
                    newEnemy = new Enemy(EnemyType.Sniper);
                    break;
                case EnemyType.Mountain:
                    newEnemy = new Enemy(EnemyType.Mountain);
                    break;
                case EnemyType.Witch:
                    newEnemy = new Enemy(EnemyType.Witch);
                    break;
                case EnemyType.Medic:
                    newEnemy = new Enemy(EnemyType.Medic);
                    break;
                case EnemyType.Warlord:
                    newEnemy = new Enemy(EnemyType.Warlord);
                    break;
            }

            scenario.AddEnemy(newEnemy);
            return newEnemy;
        }

        public bool ReachedMaxEncounterSize()
        {
            return _enemies.Count == MaxEncounterSize;
        }

        private static List<EnemyTemplate> _enemyTypes = EnemyTemplate.GetEnemyTypes();

        public static CombatScenario Generate(int difficulty, int size)
        {
            CombatScenario scenario = new CombatScenario();
            for (int i = 0; i < size; ++i)
            {
                Helper.Shuffle(ref _enemyTypes);
                foreach (EnemyTemplate t in _enemyTypes)
                {
                    if (size < t.Value) continue;
                    Enemy e = AddEnemy(t.EnemyType, scenario);
                    int armourPivot = difficulty / 2;
                    int minArmour = armourPivot - 1;
                    if (minArmour < 0) minArmour = 0;
                    int maxArmour = armourPivot + 1;
                    if (maxArmour > 10) maxArmour = 10;
                    Armour a = new Armour("armour", 1, "", Random.Range(minArmour, maxArmour), 0, 0, 0, 0);
                    e.EquipArmour(a);
                    if (scenario.ReachedMaxEncounterSize()) break;
                    difficulty -= t.Value;
                }
            }

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