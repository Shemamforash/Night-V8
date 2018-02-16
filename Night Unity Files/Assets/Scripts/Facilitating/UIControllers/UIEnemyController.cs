using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIEnemyController : MonoBehaviour, ICombatListener
    {
        public static readonly List<DetailedEnemyCombat> Enemies = new List<DetailedEnemyCombat>();

        public void EnterCombat()
        {
            CombatManager.CurrentScenario.Enemies().ForEach(e =>
            {
                if(!e.IsDead) AddEnemy(e.CreateUi(transform));
            });
        }

        public void UpdateCombat()
        {
        }

        public static void Select(float direction)
        {
            if (direction > 0)
            {
                TrySelectAtDistance(Enemies.IndexOf(CombatManager.Player.CurrentTarget), -1);
            }
            else
            {
                TrySelectAtDistance(Enemies.IndexOf(CombatManager.Player.CurrentTarget), 1);
            }
        }
        
        public void ExitCombat()
        {
            Enemies.ForEach(e =>
            {
                e.ExitCombat();
                Destroy(e.gameObject);
            });
            Enemies.Clear();
        }

        public void QueueEnemyToAdd(EnemyType type)
        {
            Enemy e = new Enemy(type);
            DetailedEnemyCombat enemyUi = e.CreateUi(transform);
            enemyUi.Alert();
            enemyUi.Position.SetCurrentValue(CombatManager.VisibilityRange + 5f);
            AddEnemy(enemyUi);
            CombatManager.CurrentScenario.AddEnemy(e);
        }

        public static bool AllEnemiesGone()
        {
            return Enemies.Count == 0;
        }

        public static void AlertAll()
        {
            Enemies.ForEach(e => e.Alert());
        }
    
        private static bool TrySelectAtDistance(int index, int distance)
        {
            if (index + distance >= Enemies.Count || index + distance < 0) return false;
            DetailedEnemyCombat enemy = Enemies[index + distance];
            if (enemy == null) return false;
            enemy.PrimaryButton.Button().Select();
            return true;
        }

        public static void Remove(DetailedEnemyCombat enemy)
        {
            int enemyPosition = Enemies.IndexOf(enemy);
            int distance = 1;
            while (distance < Enemies.Count)
            {
                if (TrySelectAtDistance(enemyPosition, distance) || TrySelectAtDistance(enemyPosition, -distance))
                {
                    break;
                }

                ++distance;
            }
            Enemies.Remove(enemy);
        }

        private static void AddEnemy(DetailedEnemyCombat e)
        {
            Enemies.Add(e);
            if (Enemies.Count == 1) e.PrimaryButton.Button().Select();
        }

        public static DetailedEnemyCombat NearestEnemy()
        {
            DetailedEnemyCombat nearestEnemy = null;
            Enemies.ForEach(e =>
            {
                if (!e.InCombat()) return;
                if (nearestEnemy == null)
                {
                    nearestEnemy = e;
                    return;
                }

                if (e.Position.CurrentValue() < nearestEnemy.Position.CurrentValue())
                {
                    nearestEnemy = e;
                }
            });
            return nearestEnemy;
        }
    }
}