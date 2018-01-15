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
        private bool _finished = false;

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
                scenario.AddEnemy(new Fighter());
                scenario.AddEnemy(new Warlord());
            }
            for (int i = 0; i < size; ++i)
            {
                int rand = Random.Range(0, 5);
                switch (rand)
                {
                    case 0:
                        scenario.AddEnemy(new Watcher());
                        break;
                    case 1:
                        scenario.AddEnemy(new Grazer());
                        break;
                    case 2:
                        scenario.AddEnemy(new Fighter());
                        break;
                    case 3:
                        scenario.AddEnemy(new Martyr());
                        break;
                    case 4:
                        scenario.AddEnemy(new Medic());
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