﻿using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Medic : ArmedBehaviour
    {
        private static GameObject _healPrefab;
        private float _healCooldown;
        private bool _goingToHeal;
        private bool _healing;
        private const float HealDistance = 0.9f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (_healPrefab == null) _healPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Heal Indicator");
        }

        private void ResetCooldown()
        {
            _healCooldown = Random.Range(5, 10);
        }

        public override void MyUpdate()
        {
            CheckForNullTarget();
            base.MyUpdate();
            if ( _healing) return;
            UpdateHealCooldown();
            TryHeal();
        }

        private void CheckForNullTarget()
        {
            if (GetTarget() != null) return;
            _goingToHeal = false;
            SetTarget(PlayerCombat.Instance);
        }

        private void TryHeal()
        {
            if (!_goingToHeal) return;
            if (DistanceToTarget() > HealDistance / 2f) return;
            Heal();
        }

        protected override void Aim()
        {
            if (GetTarget() is UnarmedBehaviour) return;
            base.Aim();
        }

        private void UpdateHealCooldown()
        {
            if (_goingToHeal) return;
            if (_healCooldown < 0)
            {
                FindTargetsToHeal();
                return;
            }

            _healCooldown -= Time.deltaTime;
        }

        private List<EnemyBehaviour> GetEnemiesNearby()
        {
            List<CanTakeDamage> chars = CombatManager.Instance().GetEnemiesInRange(transform.position, 5f);
            List<EnemyBehaviour> enemiesNearby = new List<EnemyBehaviour>();
            chars.ForEach(c =>
            {
                EnemyBehaviour behaviour = c as EnemyBehaviour;
                if (behaviour == null) return;
                if (behaviour is Martyr) return;
                if (behaviour.HealthController.GetNormalisedHealthValue() > 0.75f) return;
                enemiesNearby.Add(behaviour);
            });
            enemiesNearby.Sort((a, b) =>
            {
                float enemyHealthA = a.HealthController.GetCurrentHealth();
                float enemyHealthB = a.HealthController.GetCurrentHealth();
                return enemyHealthA.CompareTo(enemyHealthB);
            });
            return enemiesNearby;
        }

        private void SetHealTarget(EnemyBehaviour enemy)
        {
            SetTarget(enemy);
            MinDistance = HealDistance;
            MaxDistance = 0f;
            _goingToHeal = true;
        }

        private void FindTargetsToHeal()
        {
            List<EnemyBehaviour> enemiesNearby = GetEnemiesNearby();
            if (enemiesNearby.Count == 0) SetTarget(PlayerCombat.Instance);
            else SetHealTarget(enemiesNearby[0]);
        }

        private void Heal()
        {
            _goingToHeal = false;
            _healing = true;
            SkillAnimationController.Create(transform, "Medic", 1.5f, () =>
            {
                CombatManager.Instance().GetEnemiesInRange(transform.position, 1f).ForEach(e =>
                {
                    EnemyBehaviour enemy = e as EnemyBehaviour;
                    if (enemy == null) return;
                    int healAmount = (int) (enemy.HealthController.GetMaxHealth() * 0.2f);
                    enemy.HealthController.Heal(healAmount);
                    GameObject healObject = Instantiate(_healPrefab);
                    healObject.transform.position = enemy.transform.position;
                    healObject.transform.localScale = Vector3.one;
                });
                SetTarget(PlayerCombat.Instance);
                TryFire();
                ResetCooldown();
                CalculateMaxMinDistance();
                _healing = false;
            });
        }

        protected override void TryFire()
        {
            if (!(GetTarget() is PlayerCombat || GetTarget() is ShelterCharacterBehaviour)) return;
            base.TryFire();
        }
    }
}