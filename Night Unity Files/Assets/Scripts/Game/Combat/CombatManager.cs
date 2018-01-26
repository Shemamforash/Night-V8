using System;
using System.Collections.Generic;
using System.Linq;
using Assets;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.World;
using Game.World.Environment_and_Weather;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat
{
    public partial class CombatManager : Menu
    {
        private static Number _strengthText;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        private static readonly List<Enemy> Enemies = new List<Enemy>();
        private static readonly List<Grenade> Grenades = new List<Grenade>();
        private static Enemy _currentTarget;
        private static Player _player;
        private static CombatScenario _currentScenario;
        public static List<Enemy> _enemiesToAdd = new List<Enemy>();
        private static List<Grenade> _grenadesToRemove = new List<Grenade>();
        private static bool _inMelee;
        public static int VisibilityRange;
        private const int MaxEncounterSize = 6;

        public static List<Enemy> GetEnemies()
        {
            return Enemies;
        }

//        public static EnemyPlayerenemy Findenemy(Character character)
//        {
//            if(character is Enemy) return Enemies.FirstOrDefault(e => e == character);
//            return Enemies.FirstOrDefault(e => e == _currentTarget);
//        }

        public void Update()
        {
            if (MeleeController.InMelee) return;
            CombatCooldowns.UpdateCooldowns();
            Enemies.ForEach(r =>
            {
                if (r.IsDead) return;
                r.Update();
            });
            Grenades.ForEach(g => { g.Update(); });
            _grenadesToRemove.ForEach(g => Grenades.Remove(g));
            _grenadesToRemove.Clear();
            _player.Update();
            _enemiesToAdd.ForEach(AddEnemy);
            _enemiesToAdd.Clear();
            _player.RageController.Decrease();
        }

        public static void EngageMelee(Enemy e)
        {
            MeleeController.StartMelee(e);
        }

        public static void DisengagePlayerInput()
        {
            InputHandler.UnregisterInputListener(_player);
        }

        public static void EngagePlayerInput()
        {
            InputHandler.RegisterInputListener(_player);
        }

        public static void CheckPlayerOverlappingEnemy()
        {
            Enemy nearestEnemy = NearestEnemy();
            if (nearestEnemy.Position.CurrentValue() > _player.Position.CurrentValue()) return;
            _player.Position.SetCurrentValue(nearestEnemy.Position.CurrentValue());
            EngageMelee(nearestEnemy);
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

        public static void CheckPlayerFled()
        {
            Enemy nearestEnemy = NearestEnemy();
            if (nearestEnemy?.DistanceToPlayer >= VisibilityRange + 20)
            {
                //failcombat
                ExitCombat();
            }
        }
        
        public static void CheckToEndCombatByFleeing()
        {
            if (Enemies.All(e => e.HasFled) && Enemies.Count != 0)
            {
                //succed combat
                ExitCombat();
            }
        }

        public static void CheckEnemyFled(Enemy enemy)
        {
            if (enemy.DistanceToPlayer >= VisibilityRange + 20)
            {
                EnemyList.Remove(enemy.EnemyView);
            }
        }

        public static void CheckEnemyOverlappingPlayer(Enemy e)
        {
            float playerPosition = _player.Position.CurrentValue();
            Debug.Log(e.IsDead);
            if (!e.InCombat()) return;
            if (e.Position.CurrentValue() > playerPosition) return;
            e.Position.SetCurrentValue(playerPosition);
            EngageMelee(e);
        }

        public static void RemoveGrenade(Grenade g)
        {
            _grenadesToRemove.Add(g);
            GrenadeList.Remove(g.GrenadeView);
        }

        public static void QueueEnemyToAdd(Enemy e)
        {
            _enemiesToAdd.Add(e);
            e.Alert();
            _currentScenario.AddEnemy(e);
        }

        public static void AddGrenade(Grenade g)
        {
            Grenades.Add(g);
            GrenadeList.AddItem(g);
        }

        public static void ResetCombat()
        {
            Enemies.Clear();
            _currentTarget = null;
            CombatCooldowns.Clear();
        }

        public static void EnterCombat(Player player, CombatScenario scenario)
        {
            WorldState.Pause();
//            VisibilityRange = (int) (100 * WeatherManager.Instance().CurrentWeather().GetVisibility());
            VisibilityRange = 75;
            _currentScenario = scenario;
            _player = player;
            _player.HealthController.EnterCombat();
            ResetCombat();
            UIMagazineController.SetWeapon(_player.Weapon());
            _playerName.text = _player.Name;
            EnemyList.Clear();
            UpdatePlayerHealth();
            InputHandler.RegisterInputListener(_player);
            scenario.Enemies().ForEach(AddEnemy);
            MenuStateMachine.ShowMenu("Combat Menu");
        }

        public static Player Player()
        {
            return _player;
        }

        private static void AddEnemy(Enemy e)
        {
            Enemies.Add(e);
            e.HealthController.EnterCombat();
            EnemyList.AddItem(e);
            if (EnemyList.Items.Count == 1) SetTarget((Enemy) EnemyList.Items[0].GetLinkedObject());
        }

        private static void ExitCombat()
        {
            WorldState.UnPause();
            MenuStateMachine.ShowMenu("Game Menu");
            InputHandler.UnregisterInputListener(_player);
            if (Enemies.All(e => e.IsDead))
            {
                _currentScenario.FinishCombat();
            }

            _player.HealthController.ExitCombat();
        }

        public static void SetPlayerHealthText(float f)
        {
            _strengthText.SetCurrentValue(_strengthText.CurrentValue() - f);
        }

        public static Character GetTarget(Character c)
        {
            if (c is Player)
            {
                return _currentTarget;
            }

            return _player;
        }

        public static void Flee(Enemy enemy)
        {
            enemy.HasFled = true;
            CheckCombatEnd();
        }

        public static void CheckCombatEnd()
        {
            if (Enemies.All(e => !e.InCombat()))
            {
                ExitCombat();
            }
        }

        public static float DistanceBetween(Character origin, Character target)
        {
            return DistanceBetween(origin.Position.CurrentValue(), target);
        }

        public static float DistanceBetween(float originPosition, Character target)
        {
            return Math.Abs(originPosition - target.Position.CurrentValue());
        }

        public static Enemy GetCurrentTarget()
        {
            return _currentTarget;
        }

        public static List<Enemy> GetEnemiesBehindTarget(Enemy target)
        {
            List<Enemy> enemiesBehindTarget = new List<Enemy>();
            foreach (Enemy enemy in Enemies)
            {
                if (enemy == target) continue;
                if (enemy.Position > target.Position)
                {
                    enemiesBehindTarget.Add(enemy);
                }
            }

            return enemiesBehindTarget;
        }

        public static List<Character> GetCharactersInRange(Character target, float range)
        {
            List<Character> charactersInRange = GetCharactersInRange(target.Position.CurrentValue(), range);
            charactersInRange.Remove(target);
            return charactersInRange;
        }

        public static List<Character> GetCharactersInRange(float position, float range)
        {
            List<Character> charactersInRange = new List<Character>();
            if (Mathf.Abs(_player.Position.CurrentValue() - position) <= range)
            {
                charactersInRange.Add(_player);
            }

            foreach (Enemy enemy in Enemies)
            {
                if (DistanceBetween(position, enemy) <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }

        public static bool ReachedMaxEncounterSize()
        {
            return Enemies.Count == MaxEncounterSize;
        }
    }
}