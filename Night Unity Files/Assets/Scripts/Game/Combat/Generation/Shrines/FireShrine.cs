using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class FireShrine : ShrineBehaviour
    {
        private readonly List<FireBehaviour> _fires = new List<FireBehaviour>();


        protected override IEnumerator StartShrine()
        {
            int numberOfEnemies = 1;
            float angleSize = 360f / numberOfEnemies;
            for (int i = 0; i < numberOfEnemies; ++i)
            {
                float angle = Random.Range(i * angleSize, (i + 1) * angleSize);
                float radius = Random.Range(3f, 5f);
                Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, radius, transform.position);
                EnemyBehaviour b = CombatManager.SpawnEnemy(EnemyType.Maelstrom, position);
                AddEnemy(b);
            }

            int fireCount = 30;
            for (int i = 0; i < fireCount; ++i)
            {
                Vector2 firePosition = AdvancedMaths.CalculatePointOnCircle(360f / fireCount * i, 6.5f, transform.position);
                _fires.Add(FireBehaviour.Create(firePosition, 1f, true, false));
            }

            float roundTime = numberOfEnemies * 20;
            float currentTime = roundTime;
            float timeToNextBurst = Random.Range(1f, 3f);

            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                timeToNextBurst -= Time.deltaTime;
                if (timeToNextBurst < 0)
                {
                    timeToNextBurst = Random.Range(0.5f, 2f);
                    MakeFireBurst();
                }

                UpdateCountdown(currentTime, roundTime);
                if (EnemiesDead())
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
            if (!EnemiesDead()) Fail();
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
                FireBehaviour.Create(position, 0.25f, false, false);
            }
        }
    }
}