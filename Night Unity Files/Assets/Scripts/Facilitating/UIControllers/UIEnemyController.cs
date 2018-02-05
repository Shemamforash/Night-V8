using System;
using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions;

public class UIEnemyController : MonoBehaviour, ICombatListener
{
    private static MenuList _enemyList;
    private static readonly List<Enemy> EnemiesToAdd = new List<Enemy>();
    public static readonly List<Enemy> Enemies = new List<Enemy>();
    private const int MaxEncounterSize = 6;

    private void Awake()
    {
        _enemyList = GetComponent<MenuList>();
        CombatManager.RegisterCombatListener(this);
    }

    public void EnterCombat()
    {
        CombatManager.CurrentScenario.Enemies().ForEach(e =>
        {
            AddEnemy(e);
            e.EnterCombat();
        });
    }

    public void UpdateCombat()
    {
        if (MeleeController.InMelee) return;
        List<Enemy> updatedEnemies = new List<Enemy>();
        int totalEnemies = Enemies.Count;
        for (int i = 0; i < totalEnemies; ++i)
        {
            Enemy e = Enemies[i];
            if (updatedEnemies.Contains(e)) continue;
            updatedEnemies.Add(e);
            if(e.IsDead) Debug.Log(e.Name);
            try
            {
                e.UpdateCombat();
            }
            catch (InvalidOperationException)
            {
                i = 0;
                --totalEnemies;
            }
        }
        EnemiesToAdd.ForEach(AddEnemy);
        EnemiesToAdd.Clear();
    }

    public void ExitCombat()
    {
        Enemies.ForEach(e =>
        {
            e.ExitCombat();
        });
        Enemies.Clear();
        _enemyList.Clear();
    }

    public static void QueueEnemyToAdd(Enemy e)
    {
        EnemiesToAdd.Add(e);
        e.Alert();
        CombatManager.CurrentScenario.AddEnemy(e);
    }

    public static bool ReachedMaxEncounterSize()
    {
        return Enemies.Count == MaxEncounterSize;
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
        if (index + distance >= _enemyList.Items.Count || index + distance < 0) return false;
        Enemy enemy = (Enemy) _enemyList.Items[index + distance].GetLinkedObject();
        if (enemy == null) return false;
        enemy.EnemyView.Select();
        return true;
    }

    public static void Remove(Enemy enemy)
    {
        int enemyPosition = _enemyList.Items.IndexOf(enemy.EnemyView);
        int distance = 1;
        while (distance < _enemyList.Items.Count)
        {
            if (TrySelectAtDistance(enemyPosition, distance) || TrySelectAtDistance(enemyPosition, -distance))
            {
                break;
            }

            ++distance;
        }
        _enemyList.Remove(enemy.EnemyView);
        enemy.ExitCombat();
        Enemies.Remove(enemy);
    }

    private static void AddEnemy(Enemy e)
    {
        Enemies.Add(e);
        e.HealthController.EnterCombat();
        _enemyList.AddItem(e);
        if (_enemyList.Items.Count == 1) TrySelectAtDistance(0, 0);
    }

    public static Enemy NearestEnemy()
    {
        Enemy nearestEnemy = null;
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