using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Humans;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Combat
{
    public class CombatScenario : IPersistenceTemplate
    {
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private bool _finished;

        public void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
        }

        private static float RandomPosition()
        {
            return Random.Range(30f, 60f);
        }
        
        public static CombatScenario Generate(int size = 0)
        {
            CombatScenario scenario = new CombatScenario();
            if (size == 0)
            {
//                scenario.AddEnemy(new Sniper(RandomPosition()));
//                scenario.AddEnemy(new Martyr(RandomPosition()));
//                scenario.AddEnemy(new Medic(RandomPosition()));
//                scenario.AddEnemy(new Mountain(RandomPosition()));
                scenario.AddEnemy(new Sentinel(RandomPosition()));
//                scenario.AddEnemy(new Warlord(RandomPosition()));
                scenario.AddEnemy(new Witch(RandomPosition()));
//                scenario.AddEnemy(new Brawler(RandomPosition()));
            }
            for (int i = 0; i < size; ++i)
            {
                int rand = Random.Range(0, 5);
                switch (rand)
                {
                    case 0:
                        scenario.AddEnemy(new Watcher(Random.Range(20f, 40f)));
                        break;
                    case 1:
                        scenario.AddEnemy(new Grazer(Random.Range(20f, 40f)));
                        break;
                    case 2:
                        scenario.AddEnemy(new Sentinel(Random.Range(20f, 40f)));
                        break;
                    case 3:
                        scenario.AddEnemy(new Martyr(Random.Range(20f, 40f)));
                        break;
                    case 4:
                        scenario.AddEnemy(new Medic(Random.Range(20f, 40f)));
                        break;
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