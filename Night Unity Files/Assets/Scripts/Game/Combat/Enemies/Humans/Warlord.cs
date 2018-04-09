﻿using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Warlord : EnemyBehaviour
    {
        private float _reinforceCallTime;
        private float _reinforceDuration = 5f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
//            MinimumFindCoverDistance = 10;
//            HealthController.AddOnTakeDamage(a =>
//            {
//                if (CombatManager.CurrentScenario.ReachedMaxEncounterSize()) return;
//                float normalHealthBefore = (HealthController.GetCurrentHealth() + a) / HealthController.GetMaxHealth();
//                float currentNormalHealth = HealthController.GetNormalisedHealthValue();
//                if (normalHealthBefore > 0.25f && currentNormalHealth <= 0.25f
//                    || normalHealthBefore > 0.5f && currentNormalHealth <= 0.5f
//                    || normalHealthBefore > 0.75f && currentNormalHealth <= 0.75f)
//                {
//                    CurrentAction = SummonEnemies;
//                    _reinforceCallTime = _reinforceDuration;
//                }
//            });
        }

        private void SummonEnemies()
        {
            SetActionText("Reinforcing");
            _reinforceCallTime -= Time.deltaTime;
            if (!(_reinforceCallTime <= 0)) return;

            CombatManager.EnemyController.QueueEnemyToAdd(EnemyType.Sentinel);
            ChooseNextAction();
//            switch (Random.Range(0, 4))
//            {
//                case 0:
//                    CombatManager.QueueEnemyToAdd(new Fighter());
//                    break;
//                case 1:
//                    CombatManager.QueueEnemyToAdd(new Sniper());
//                    break;
//                case 2:
//                    CombatManager.QueueEnemyToAdd(new Martyr());
//                    break;
//                case 3:
//                    CombatManager.QueueEnemyToAdd(new Medic());
//                    break;
//            }
        }
    }
}