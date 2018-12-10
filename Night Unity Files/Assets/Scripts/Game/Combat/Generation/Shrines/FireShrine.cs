using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class FireShrine : ShrineBehaviour
    {
        private readonly List<FireBehaviour> _fires = new List<FireBehaviour>();
        private float _timeToNextEnemy;
        private int _maxEnemies;
        private float _timeToNextBurst;

        protected override void StartChallenge()
        {
            StartCoroutine(SpawnFire());
        }

        protected override string GetInstructionText() => "Kill all enemies within the time limit";

        private void SpawnEnemy()
        {
            float angle = Random.Range(0, 360);
            float radius = Random.Range(3f, 5f);
            Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, radius, Vector2.zero);
            EnemyType typeToSpawn = Helper.RollDie(0, 2) ? EnemyType.Maelstrom : EnemyType.Shadow;
            EnemyBehaviour b = CombatManager.SpawnEnemy(typeToSpawn, position);
        }

        private void CreateFires()
        {
            int fireCount = 30;
            for (int i = 0; i < fireCount; ++i)
            {
                Vector2 firePosition = AdvancedMaths.CalculatePointOnCircle(360f / fireCount * i, 6.5f, transform.position);
                _fires.Add(FireBehaviour.Create(firePosition));
            }
        }

        private void TrySpawnEnemy()
        {
            if (CombatManager.Enemies().Count < _maxEnemies)
            {
                _timeToNextEnemy -= Time.deltaTime;
                if (_timeToNextEnemy < 0)
                {
                    _timeToNextEnemy = Random.Range(0.5f, 1.5f);
                    SpawnEnemy();
                }
            }
        }

        private void TryCreateFireBurst()
        {
            _timeToNextBurst -= Time.deltaTime;
            if (_timeToNextBurst < 0)
            {
                _timeToNextBurst = Random.Range(0.5f, 2f);
                MakeFireBurst();
            }
        }

        private IEnumerator SpawnFire()
        {
            CreateFires();

            int numberOfEnemies = (int) (WorldState.Difficulty() / 5f + 10);
            _maxEnemies = numberOfEnemies / 10;

            float roundTime = numberOfEnemies * 10;
            float currentTime = roundTime;

            _timeToNextBurst = Random.Range(1f, 3f);
            _timeToNextEnemy = Random.Range(0.5f, 1.5f);

            while (currentTime > 0)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                currentTime -= Time.deltaTime;
                TrySpawnEnemy();
                TryCreateFireBurst();
                UpdateCountdown(currentTime, roundTime);
                if (CombatManager.ClearOfEnemies())
                {
                    Succeed();
                    break;
                }

                yield return null;
            }

            EndChallenge();
        }

        protected override void EndChallenge()
        {
            _fires.ForEach(f => f.LetDie());
            if (!CombatManager.ClearOfEnemies()) Fail();
            base.EndChallenge();
        }

        private void MakeFireBurst()
        {
            float angleFrom = Random.Range(0, 360);
            float angleTo = angleFrom + Random.Range(90, 270);
            if (angleFrom > 360) angleFrom -= 360;
            Vector2 from = AdvancedMaths.CalculatePointOnCircle(angleFrom, 7.5f, transform.position);
            Vector2 to = AdvancedMaths.CalculatePointOnCircle(angleTo, 7.5f, transform.position);
            for (float pos = 0; pos <= 1; pos += 0.025f)
            {
                Vector2 position = AdvancedMaths.PointAlongLine(from, to, pos);
                TrailFireBehaviour.Create(position);
            }
        }
    }
}