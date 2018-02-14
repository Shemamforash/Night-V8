using System;
using System.Collections.Generic;
using Assets;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CanvasGroup CombatCanvas, CentralView;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        public static CombatScenario CurrentScenario;
        private static bool _inMelee;
        public static int VisibilityRange;
        private static readonly List<ICombatListener> CombatListeners = new List<ICombatListener>();
        public static PlayerCombat Player;

        public void Awake()
        {
            Player = Helper.FindChildWithName<PlayerCombat>(gameObject, "Player");
            CombatCanvas = Helper.FindChildWithName<CanvasGroup>(gameObject, "Combat Canvas");
        }

        public static void RegisterCombatListener(ICombatListener listener)
        {
            CombatListeners.Add(listener);
        }

        private static readonly List<ICombatListener> _listenersToRemove = new List<ICombatListener>();

        public static void UnregisterCombatListener(ICombatListener listener)
        {
            _listenersToRemove.Add(listener);
        }

        public void Update()
        {
            if (MeleeController.InMelee) return;
            CombatListeners.ForEach(l => l.UpdateCombat());
            _listenersToRemove.ForEach(l => CombatListeners.Remove(l));
            Player.UpdateCombat();
            CombatCooldowns.UpdateCooldowns();
        }

        public static void CheckPlayerFled()
        {
            DetailedEnemyCombat nearestEnemy = UIEnemyController.NearestEnemy();
            if (nearestEnemy?.DistanceToPlayer >= VisibilityRange + 20)
            {
                FailCombat();
            }
        }

        public static void CheckEnemyFled(DetailedEnemyCombat enemy)
        {
            if (enemy.DistanceToPlayer >= VisibilityRange + 20)
            {
                UIEnemyController.Remove(enemy);
            }
        }

        public static void EnterCombat(Player player, CombatScenario scenario)
        {
            WorldState.Pause();
//            VisibilityRange = (int) (100 * WeatherManager.Instance().CurrentWeather().GetVisibility());
            VisibilityRange = 75;
            CurrentScenario = scenario;
            Player.SetPlayer(player);
            CombatCooldowns.Clear();
            MenuStateMachine.ShowMenu("Combat Menu");
            CombatListeners.ForEach(l => l.EnterCombat());
        }

        public static void SucceedCombat()
        {
            if (SceneManager.GetActiveScene().name == "Combat Tester")
            {
                MenuStateMachine.ShowMenu("Next Level");
            }
            else
            {
                MenuStateMachine.ShowMenu("Game Menu");
                if (UIEnemyController.AllEnemiesGone())
                {
                    CurrentScenario.FinishCombat();
                }
            }

            ExitCombat();
        }

        public static void FailCombat()
        {
            if (SceneManager.GetActiveScene().name == "Combat Tester")
            {
                MenuStateMachine.ShowMenu("Minigame Menu");
            }
            else
            {
                MenuStateMachine.ShowMenu("Game Menu");
            }

            ExitCombat();
        }

        private static void ExitCombat()
        {
            WorldState.UnPause();
            CombatListeners.ForEach(l => l.ExitCombat());
            Player.ExitCombat();
        }

        public static void Flee(DetailedEnemyCombat enemy)
        {
            enemy.HasFled = true;
            CheckCombatEnd();
        }

        public static void CheckCombatEnd()
        {
            if (UIEnemyController.AllEnemiesGone())
            {
                SucceedCombat();
            }
        }

        public static float DistanceBetween(float originPosition, CharacterCombat target)
        {
            return Math.Abs(originPosition - target.Position.CurrentValue());
        }

        public static List<DetailedEnemyCombat> GetEnemiesBehindTarget(DetailedEnemyCombat target)
        {
            List<DetailedEnemyCombat> enemiesBehindTarget = new List<DetailedEnemyCombat>();
            foreach (DetailedEnemyCombat enemy in UIEnemyController.Enemies)
            {
                if (enemy == target) continue;
                if (enemy.Position > target.Position)
                {
                    enemiesBehindTarget.Add(enemy);
                }
            }

            return enemiesBehindTarget;
        }

        public static List<CharacterCombat> GetCharactersInRange(float position, float range)
        {
            List<CharacterCombat> charactersInRange = new List<CharacterCombat>();
            if (Mathf.Abs(Player.Position.CurrentValue() - position) <= range)
            {
                charactersInRange.Add(Player);
            }

            foreach (DetailedEnemyCombat enemy in UIEnemyController.Enemies)
            {
                if (DistanceBetween(position, enemy) <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }
    }
}