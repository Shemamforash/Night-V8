using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Spawn : BasicAttackBehaviour
    {
        private EnemyType _spawnType;
        private int _countMin, _countMax;
        
        public void Initialise(EnemyType spawnType, float maxTimer, int countMin, int countMax = -1)
        {
            if (countMax == -1) countMax = countMin;
            _countMin = countMin;
            _countMax = countMax;
            _spawnType = spawnType;
            MaxTimer = maxTimer;
        }

        protected override void Attack()
        {
            int enemiesToSpawn = Random.Range(_countMin + 1, _countMax + 1);
            for (int i = _countMin; i < enemiesToSpawn; ++i)
            {
                Cell c = PathingGrid.GetCellNearMe(Enemy.CurrentCell(), 2f);
                EnemyBehaviour ghoul = CombatManager.QueueEnemyToAdd(_spawnType);
                ghoul.gameObject.transform.position = c.Position;
            }
        }
    }
}