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
        
        public static CombatScenario Generate(int size = 0)
        {
            CombatScenario scenario = new CombatScenario();
            if (size == 0)
            {
//                scenario.AddEnemy(new Sniper());
//                scenario.AddEnemy(new Martyr());
//                scenario.AddEnemy(new Medic());
//                scenario.AddEnemy(new Fighter(Random.Range(20f, 40f)));
                scenario.AddEnemy(new Warlord(Random.Range(20f, 40f)));
                scenario.AddEnemy(new Witch(Random.Range(20f, 40f)));
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
                        scenario.AddEnemy(new Fighter(Random.Range(20f, 40f)));
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