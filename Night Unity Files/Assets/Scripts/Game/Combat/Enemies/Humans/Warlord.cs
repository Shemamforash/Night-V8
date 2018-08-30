using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Warlord : ArmedBehaviour
    {
        private const float ReinforceCallTime = 3f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            HealthController.AddOnTakeDamage(a =>
            {
                float normalHealthBefore = (HealthController.GetCurrentHealth() + a) / HealthController.GetMaxHealth();
                float currentNormalHealth = HealthController.GetNormalisedHealthValue();
                if (normalHealthBefore > 0.25f && currentNormalHealth <= 0.25f
                    || normalHealthBefore > 0.5f && currentNormalHealth <= 0.5f
                    || normalHealthBefore > 0.75f && currentNormalHealth <= 0.75f)
                {
                    CurrentAction = null;
                    SetActionText("Reinforcing");
                    SkillAnimationController.Create("Warlord", ReinforceCallTime, SummonEnemies);
                }
            });
        }

        private void SummonEnemies()
        {
            CombatManager.QueueEnemyToAdd(EnemyType.Sentinel);
            switch (Random.Range(0, 4))
            {
                case 0:
                    CombatManager.QueueEnemyToAdd(EnemyType.Sentinel);
                    break;
                case 1:
                    CombatManager.QueueEnemyToAdd(EnemyType.Sniper);
                    break;
                case 2:
                    CombatManager.QueueEnemyToAdd(EnemyType.Martyr);
                    break;
                case 3:
                    CombatManager.QueueEnemyToAdd(EnemyType.Medic);
                    break;
            }
            TryFire();
        }
    }
}