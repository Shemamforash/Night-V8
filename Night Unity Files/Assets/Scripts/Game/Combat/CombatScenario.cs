using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using UnityEngine;

namespace Game.Combat
{
    public class CombatScenario
    {
        private Player _playerCharacter;
        private readonly List<Enemy> _enemies = new List<Enemy>();

        public void Remove(Enemy enemy)
        {
            _enemies.Remove(enemy);
        }

        private void AddEnemy(Enemy enemy)
        {
            _enemies.Add(enemy);
        }

        public void SetCharacter(Player playerCharacter)
        {
            _playerCharacter = playerCharacter;
        }

        public static CombatScenario Generate(int size)
        {
            CombatScenario scenario = new CombatScenario();
            for (int i = 0; i < size; ++i)
            {
                int rand = Random.Range(0, 2);
//                scenario.AddEnemy(new Sniper(true));
                scenario.AddEnemy(new Sniper(false));
                scenario.AddEnemy(new Martyr());
                scenario.AddEnemy(new Medic());
//                switch (rand)
//                {
//                    case 0:
//                        scenario.AddEnemy(new Watcher());
//                        break;
//                    case 1:
//                        scenario.AddEnemy(new Grazer());
//                        break;
//                    case 2:
//                        scenario.AddEnemy(new Fighter());
//                        break;
//                }
            }
            return scenario;
        }

        public Player Player()
        {
            return _playerCharacter;
        }

        public List<Enemy> Enemies()
        {
            return _enemies;
        }
    }
}