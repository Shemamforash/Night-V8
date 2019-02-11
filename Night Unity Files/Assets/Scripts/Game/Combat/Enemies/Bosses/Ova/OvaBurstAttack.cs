using System.Collections;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class OvaBurstAttack : TimedAttackBehaviour
    {
        protected override void Attack()
        {
            StartCoroutine(DoBurst());
        }

        private IEnumerator DoBurst()
        {
            Paused = true;
            float explosionCount = 30;
            float angleInterval = 360f / explosionCount;
            int offset = 0;
            while (offset < 6)
            {
                for (int i = 0; i < 360; i += 72)
                {
                    float angle = (i + offset * 12) * angleInterval;
                    Vector2 direction = AdvancedMaths.CalculatePointOnCircle(angle, 1, Vector2.zero);
                    MaelstromShotBehaviour.Create(direction, (Vector2) transform.position + direction, 0.5f, false);
                }

                yield return new WaitForSeconds(0.5f);
                ++offset;
            }

            Paused = false;
        }
    }
}