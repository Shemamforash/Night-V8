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

        public void Resolve()
        {
            _playerCharacter.CombatStates.ReturnToDefault();
            _enemies.ForEach(e => e.CombatStates.ReturnToDefault());
        }

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
                if (Random.Range(0, 2) == 0)
                {
                    scenario.AddEnemy(new Watcher());
                    continue;
                }
                scenario.AddEnemy(new Grazer());
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