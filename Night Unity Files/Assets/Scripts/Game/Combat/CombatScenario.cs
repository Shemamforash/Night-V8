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

        public void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
        }

        private static float RandomPosition()
        {
            return Random.Range(30f, 60f);
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

        public static CombatScenario Generate(int difficulty)
        {
            CombatScenario scenario = new CombatScenario();
            List<int> enemyNos = new List<int> {0, 1, 2, 3, 4, 5, 6, 7};
            while (difficulty > 0)
            {
                Helper.Shuffle(ref enemyNos);
                foreach (int i in enemyNos)
                {
                    if (difficulty == 0) break;
                    switch (i)
                    {
                        case 0:
                            if (difficulty >= 1)
                            {
                                scenario.AddEnemy(new Sentinel(Random.Range(20f, 40f)));
                                difficulty -= 1;
                            }
                            break;
                        case 1:
                            if (difficulty >= 1)
                            {
                                scenario.AddEnemy(new Martyr(Random.Range(20f, 40f)));
                                difficulty -= 1;
                            }                            break;
                        case 2:
                            if (difficulty >= 2)
                            {
                                scenario.AddEnemy(new Brawler(Random.Range(20f, 40f)));
                                difficulty -= 2;
                            }                            break;
                        case 3:
                            if (difficulty >= 2)
                            {
                                scenario.AddEnemy(new Sniper(Random.Range(20f, 40f)));
                                difficulty -= 2;
                            }                            break;
                        case 4:
                            if (difficulty >= 3)
                            {
                                scenario.AddEnemy(new Medic(Random.Range(20f, 40f)));
                                difficulty -= 3;
                            }                            break;
                        case 5:
                            if (difficulty >= 3)
                            {
                                scenario.AddEnemy(new Mountain(Random.Range(20f, 40f)));
                                difficulty -= 3;
                            }                            break;
                        case 6:
                            if (difficulty >= 3)
                            {
                                scenario.AddEnemy(new Witch(Random.Range(20f, 40f)));
                                difficulty -= 3;
                            }                            break;
                        case 7:
                            if (difficulty >= 4)
                            {
                                scenario.AddEnemy(new Warlord(Random.Range(20f, 40f)));
                                difficulty -= 4;
                            }                            break;
                    }
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