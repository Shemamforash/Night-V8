using System;
using System.Collections.Generic;
using Game.Combat;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIEnemyController : MonoBehaviour, ICombatListener
    {
        private static readonly List<DetailedEnemyCombat> EnemiesToAdd = new List<DetailedEnemyCombat>();
        public static readonly List<DetailedEnemyCombat> Enemies = new List<DetailedEnemyCombat>();

        private void Awake()
        {
            CombatManager.RegisterCombatListener(this);
        }

        public void EnterCombat()
        {
            CombatManager.CurrentScenario.Enemies().ForEach(e =>
            {
                AddEnemy(e.CreateUi(transform));
            });
        }

        public void UpdateCombat()
        {
            if (MeleeController.InMelee) return;
            List<DetailedEnemyCombat> updatedEnemies = new List<DetailedEnemyCombat>();
            int totalEnemies = Enemies.Count;
            int i = totalEnemies - 1;
            while(totalEnemies > 0 && i >= 0){
                DetailedEnemyCombat e = Enemies[i];
                if (updatedEnemies.Contains(e)) continue;
                updatedEnemies.Add(e);
                if(e.IsDead) Debug.Log(e.Enemy.Name);
                try
                {
                    e.UpdateCombat();
                }
                catch (InvalidOperationException)
                {
                    i = 0;
                    --totalEnemies;
                }
                --i;
            }
            EnemiesToAdd.ForEach(AddEnemy);
            EnemiesToAdd.Clear();
        }

        public void ExitCombat()
        {
            Enemies.ForEach(e =>
            {
                e.ExitCombat();
                Destroy(e);
            });
            Enemies.Clear();
        }

        public static void QueueEnemyToAdd(DetailedEnemyCombat e)
        {
            EnemiesToAdd.Add(e);
            e.Alert();
            CombatManager.CurrentScenario.AddEnemy(e.Enemy);
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