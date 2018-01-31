using System.Collections.Generic;
using System.Linq;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

public class UIEnemyController : MonoBehaviour
{
    private static MenuList _enemyList;
    private static readonly List<Enemy> EnemiesToAdd = new List<Enemy>();
    public static readonly List<Enemy> Enemies = new List<Enemy>();
    private const int MaxEncounterSize = 6;

    private void Awake()
    {
        _enemyList = GetComponent<MenuList>();
    }

    public void Update()
    {
        if (MeleeController.InMelee) return;
        Enemies.ForEach(e =>
        {
            if (e.IsDead) return;
            e.Update();
        });
        EnemiesToAdd.ForEach(AddEnemy);
        EnemiesToAdd.Clear();
    }

    public static void EnterCombat(CombatScenario scenario)
    {
        Enemies.Clear();
        _enemyList.Clear();
        scenario.Enemies().ForEach(AddEnemy);
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

    public static bool AllEnemiesDead()
    {
        return Enemies.All(e => e.IsDead);
    }

    public static bool AllEnemiesGone()
    {
        return Enemies.All(e => !e.InCombat());
    }

    private static bool TrySelectAtDistance(int index, int distance)
    {
        if (index + distance >= _enemyList.Items.Count || index - distance < 0) return false;
        Enemy enemy = (Enemy) _enemyList.Items[index + distance].GetLinkedObject();
        if (enemy == null) return false;
        CombatManager.SetTarget(enemy);
        return true;
    }

    public static void AlertAll()
    {
        Enemies.ForEach(e => e.Alert());
    }

    public static void Remove(Enemy enemy)
    {
        int enemyPosition = _enemyList.Items.IndexOf(enemy.EnemyView);
        for (int distance = 1; distance + enemyPosition < _enemyList.Items.Count; ++distance)
        {
            if (TrySelectAtDistance(enemyPosition, distance) || TrySelectAtDistance(enemyPosition, -distance))
            {
                break;
            }
        }
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

            if (e.Position < nearestEnemy.Position)
            {
                nearestEnemy = e;
            }
        });
        return nearestEnemy;
    }
}