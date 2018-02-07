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

            //                scenario.AddEnemy(new Sniper(RandomPosition()));
//                scenario.AddEnemy(new Martyr(RandomPosition()));
//                scenario.AddEnemy(new Medic(RandomPosition()));
//                scenario.AddEnemy(new Mountain(RandomPosition()));
            scenario.AddEnemy(new Sentinel(RandomPosition()));
//                scenario.AddEnemy(new Warlord(RandomPosition()));
            scenario.AddEnemy(new Witch(RandomPosition()));
//                scenario.AddEnemy(new Brawler(RandomPosition()));
            return scenario;
        }

        private static float RandomPosition()
        {
            return Random.Range(40f, 60f);
        }
        
        private static void AddEnemy(string enemyName, CombatScenario scenario)
        {
            switch (enemyName)
            {
                case nameof(Sentinel):
                    scenario.AddEnemy(new Sentinel(RandomPosition()));
                    break;
                case nameof(Martyr):
                    scenario.AddEnemy(new Martyr(RandomPosition()));
                    break;
                case nameof(Brawler):
                    scenario.AddEnemy(new Brawler(RandomPosition()));
                    break;
                case nameof(Sniper):
                    scenario.AddEnemy(new Sniper(RandomPosition()));
                    break;
                case nameof(Mountain):
                    scenario.AddEnemy(new Mountain(RandomPosition()));
                    break;
                case nameof(Witch):
                    scenario.AddEnemy(new Witch(RandomPosition()));
                    break;
                case nameof(Medic):
                    scenario.AddEnemy(new Medic(RandomPosition()));
                    break;
                case nameof(Warlord):
                    scenario.AddEnemy(new Warlord(RandomPosition()));
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
                    AddEnemy(t.Name, scenario);
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