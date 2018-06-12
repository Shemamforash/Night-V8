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
            for (int i = _countMin; i < enemiesToSpawn; ++i)
            {
                Cell c = PathingGrid.GetCellNearMe(Enemy.CurrentCell(), 2f);
                EnemyBehaviour enemy = CombatManager.QueueEnemyToAdd(_spawnType);
                enemy.gameObject.transform.position = c.Position;
                enemy.gameObject.AddComponent<Shield>().Initialise(2f);
            }
        }
    }
}