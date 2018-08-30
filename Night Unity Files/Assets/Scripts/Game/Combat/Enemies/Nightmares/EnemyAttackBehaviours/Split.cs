using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Split : DamageThresholdAttackBehaviour
    {
        private EnemyBehaviour _enemy;
        private int _splitCountMin, _splitCountMax;
        private float _spawnForce = 10;
        private readonly List<EnemyBehaviour> _enemies = new List<EnemyBehaviour>();
        private EnemyType _enemyType;
        private BasicShrineBehaviour _shrine;
        private int _generation;

        protected override void Attack()
        {
            _enemies.Clear();
            int splitCount = Random.Range(_splitCountMin, _splitCountMax + 1);
            int newGeneration = _generation + 1;
            if (newGeneration == 3) return;
            for (int i = 0; i < splitCount; ++i)
            {
                EnemyBehaviour enemy = _enemyType == EnemyType.Decoy ? Decoy.Create(GetComponent<EnemyBehaviour>()) : CombatManager.QueueEnemyToAdd(_enemyType);
                Vector2 randomDir = AdvancedMaths.RandomVectorWithinRange(Vector3.zero, 1).normalized;
                enemy.gameObject.transform.position = transform.position;
                enemy.MovementController.AddForce(randomDir * _spawnForce);
                _enemies.Add(enemy);
                Split split = enemy.GetComponent<Split>();
                if (split != null) split._generation = newGeneration;
                if (_shrine == null) continue;
                _shrine.AddEnemy(_enemy);
            }
        }

        public void Initialise(int splitCountMin, float spawnForce, EnemyType enemyType, int healthThreshold, int splitCountMax = -1, bool activateOnDeath = false)
        {
            Initialise(healthThreshold, activateOnDeath);
            _splitCountMin = splitCountMin;
            _splitCountMax = splitCountMax == -1 ? _splitCountMin : splitCountMax;
            _spawnForce = spawnForce;
            _enemyType = enemyType;
        }

        public List<EnemyBehaviour> LastSplitEnemies()
        {
            return _enemies;
        }

        public void SetShrine(BasicShrineBehaviour shrine)
        {
            _shrine = shrine;
        }
    }
}