using Game.Combat.Generation;
using SamsHelper.Libraries;
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
            SkillAnimationController.Create(transform, "Spawn", 2f, () =>
            {
                int enemiesToSpawn = Random.Range(_countMin, _countMax + 1);
                if (CombatManager.Instance().Enemies().Count > 15) enemiesToSpawn = 0;

                for (int i = 0; i < enemiesToSpawn; ++i)
                {
                    Vector2 spawnPosition = AdvancedMaths.RandomDirection() * Random.Range(0.5f, 2f) + (Vector2) transform.position;
                    CombatManager.Instance().SpawnEnemy(_spawnType, spawnPosition);
                }
            });
        }
    }
}