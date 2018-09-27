using System.Collections.Generic;
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
        private const float HealDistance = 0.5f;

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
            base.MyUpdate();
            UpdateHealCooldown();
            TryHeal();
        }

        private void TryHeal()
        {
            if (!_goingToHeal) return;
            if (GetTarget() == null)
            {
                _goingToHeal = false;
                SetTarget(PlayerCombat.Instance);
            }
            if (DistanceToTarget() > HealDistance / 2f) return;
            Heal();
            ResetCooldown();
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

        private void FindTargetsToHeal()
        {
            List<ITakeDamageInterface> chars = CombatManager.GetEnemiesInRange(transform.position, 5f);
            List<EnemyBehaviour> enemiesNearby = new List<EnemyBehaviour>();
            chars.ForEach(c =>
            {
                EnemyBehaviour behaviour = c as EnemyBehaviour;
                if (behaviour == null) return;
                if (behaviour is Martyr) return;
                enemiesNearby.Add(behaviour);
            });
            if (enemiesNearby.Count == 0)
            {
                SetTarget(PlayerCombat.Instance);
                return;
            }

            enemiesNearby.Sort((a, b) =>
            {
                float enemyHealthA = a.HealthController.GetCurrentHealth();
                float enemyHealthB = a.HealthController.GetCurrentHealth();
                return enemyHealthA.CompareTo(enemyHealthB);
            });
            if (enemiesNearby[0].HealthController.GetNormalisedHealthValue() > 0.75f)
            {
                SetTarget(PlayerCombat.Instance);
                return;
            }

            SetTarget(enemiesNearby[0]);
            _goingToHeal = true;
        }

        private void Heal()
        {
            SkillAnimationController.Create(transform, "Medic", 1.5f, () =>
            {
                CombatManager.GetEnemiesInRange(transform.position, 2f).ForEach(e =>
                {
                    EnemyBehaviour enemy = e as EnemyBehaviour;
                    if (enemy == null) return;
                    int healAmount = (int) (enemy.HealthController.GetMaxHealth() * 0.2f);
                    enemy.HealthController.Heal(healAmount);
                    GameObject healObject = Instantiate(_healPrefab);
                    healObject.transform.position = enemy.transform.position;
                    healObject.transform.localScale = Vector3.one;
                });
                TryFire();
            });
        }
    }
}