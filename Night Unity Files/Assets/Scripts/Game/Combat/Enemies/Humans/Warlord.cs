using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
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
                    SkillAnimationController.Create(transform, "Warlord", ReinforceCallTime, SummonEnemies);
                }
            });
        }

        private void SummonEnemies()
        {
            int reinforcementSize = WorldState.Difficulty() / 10 + 2;
            List<EnemyTemplate> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
            allowedEnemies.RemoveAll(t => t.EnemyType == EnemyType.Warlord);
            List<EnemyType> enemiesToSpawn = new List<EnemyType>();
            while (reinforcementSize > 0)
            {
                allowedEnemies.Shuffle();
                foreach (EnemyTemplate template in allowedEnemies)
                {
                    if (template.Value > reinforcementSize) continue;
                    enemiesToSpawn.Add(template.EnemyType);
                    reinforcementSize -= template.Value;
                    break;
                }
            }

            enemiesToSpawn.ForEach(type =>
            {
                Vector2 spawnPosition = PathingGrid.GetCellNearMe(transform.position, 3).Position;
                SpawnTrailController.Create(transform.position, spawnPosition, type);
            });

            TryFire();
        }
    }
}