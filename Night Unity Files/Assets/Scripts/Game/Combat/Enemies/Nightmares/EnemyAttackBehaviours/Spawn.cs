using System.Collections.Generic;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Spawn : TimedAttackBehaviour
    {
        private EnemyType _spawnType;
        private int _countMin, _countMax;

        public void Initialise(EnemyType spawnType, float maxTimer, int countMin, int countMax = -1)
        {
            if (countMax == -1) countMax = countMin;
            _countMin = countMin;
            _countMax = countMax;
            _spawnType = spawnType;
            Initialise(maxTimer);
        }

        protected override void Attack()
        {
            int enemiesToSpawn = Random.Range(_countMin + 1, _countMax + 1);
            List<Cell> cellsNearMe = PathingGrid.GetCellsNearMe(Enemy.CurrentCell(), enemiesToSpawn, 2f, 0.5f);
            enemiesToSpawn = cellsNearMe.Count;
            for (int i = 0; i < enemiesToSpawn; ++i)
            {
                EnemyBehaviour enemy = CombatManager.SpawnEnemy(_spawnType, cellsNearMe[i].Position);
                enemy.Shield.Activate(2f);
            }
        }
    }
}