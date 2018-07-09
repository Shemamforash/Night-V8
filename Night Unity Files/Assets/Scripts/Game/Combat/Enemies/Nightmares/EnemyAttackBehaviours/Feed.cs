using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Feed : DamageThresholdAttackBehaviour
    {
        private int _feedingCount;

        public void DecreaseDrawLifeCount(int health)
        {
            Enemy.HealthController.Heal(health);
            --_feedingCount;
        }
        
        protected override void Attack()
        {
            List<CharacterCombat> charactersInRange = CombatManager.GetCharactersInRange(transform.position, 5f);
            int maxDraw = Random.Range(2, 5);
            foreach (CharacterCombat c in charactersInRange)
            {
                if (maxDraw == 0) return;
                FeedTarget drain = c.GetComponent<FeedTarget>();
                if (drain == null) continue;
                --maxDraw;
                drain.StartDrawLife(this);
                ++_feedingCount;
            }
        }
    }
}