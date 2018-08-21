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
            List<ITakeDamageInterface> charactersInRange = CombatManager.GetCharactersInRange(transform.position, 5f);
            int maxDraw = Random.Range(2, 5);
            foreach (ITakeDamageInterface c in charactersInRange)
            {
                CharacterCombat character = c as CharacterCombat;
                if (maxDraw == 0) return;
                if (character == null) continue; 
                FeedTarget drain = character.GetComponent<FeedTarget>();
                if (drain == null) continue;
                --maxDraw;
                drain.StartDrawLife(this);
                ++_feedingCount;
            }
        }
    }
}