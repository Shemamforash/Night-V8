using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Warlord : ArmedBehaviour
    {
        private bool _calledReinforcements;
        private const float ReinforceCallTime = 3f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            HealthController.AddOnTakeDamage(a =>
            {
                float normalHealthBefore = (HealthController.GetCurrentHealth() + a) / HealthController.GetMaxHealth();
                float currentNormalHealth = HealthController.GetNormalisedHealthValue();
                if (_calledReinforcements) return;
                if (normalHealthBefore < 0.5f || currentNormalHealth > 0.5f) return;
                CurrentAction = null;
                SkillAnimationController.Create(transform, "Warlord", ReinforceCallTime, SummonEnemies);
                _calledReinforcements = true;
            });
        }

        private void SummonEnemies()
        {
            int reinforcementSize = WorldState.Difficulty() / 10 + 2;
            List<EnemyType> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
            allowedEnemies.RemoveAll(t => t == EnemyType.Warlord);
            List<EnemyType> enemiesToSpawn = EnemyTemplate.RandomiseEnemiesToSize(allowedEnemies, reinforcementSize);
            Vector3 position = transform.position;
            enemiesToSpawn.ForEach(type =>
            {
                Vector2 spawnPosition = WorldGrid.GetCellNearMe(position, 3).Position;
                SpawnTrailController.Create(position, spawnPosition, type);
            });

            TryFire();
        }
    }
}