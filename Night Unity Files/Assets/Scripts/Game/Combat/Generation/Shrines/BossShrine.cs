using System.Collections;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class BossShrine : ShrineBehaviour
    {
        protected override IEnumerator StartShrine()
        {
            float roundTime = 60;
            float currentTime = roundTime;

            EnemyBehaviour b = CombatManager.SpawnEnemy(EnemyType.GhoulMother, transform.position);
            AddEnemy(b);
            b.gameObject.AddComponent<Bombardment>().Initialise(5, 2);
            b.gameObject.AddComponent<ErraticDash>();
            b.gameObject.AddComponent<Heavyshot>().Initialise(6, 3);

            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;

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
            if (EnemiesDead())
            {
                Succeed();
            }
            else
            {
                Fail();
            }

            base.EndChallenge();
        }
    }
}