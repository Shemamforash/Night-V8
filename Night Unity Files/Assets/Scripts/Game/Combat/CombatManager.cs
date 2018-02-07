using System;
using System.Collections.Generic;
using Assets;
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
        public static CanvasGroup CombatCanvas, PlayerCanvasGroup;

        private static TextMeshProUGUI _playerName;
        private static TextMeshProUGUI _playerHealthText;
        private static TextMeshProUGUI _coverText;

        public static UIHealthBarController PlayerHealthBar;

        private float _criticalTarget;

        private static Number _strengthText;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();

        public static Enemy CurrentTarget;
        public static Player Player;
        public static CombatScenario CurrentScenario;

        private static bool _inMelee;
        public static int VisibilityRange;
        private static readonly List<ICombatListener> _combatListeners = new List<ICombatListener>();


        public void Awake()
        {
            GameObject playerContainer = Helper.FindChildWithName(gameObject, "Player");
            PlayerCanvasGroup = playerContainer.GetComponent<CanvasGroup>();

            CombatCanvas = Helper.FindChildWithName<CanvasGroup>(gameObject, "Combat Canvas");

            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Name");
            _playerHealthText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Health");

            _coverText = Helper.FindChildWithName<TextMeshProUGUI>(playerContainer, "Cover");
            _coverText.text = "";

            PlayerHealthBar = Helper.FindChildWithName<UIHealthBarController>(playerContainer, "Health Bar");
            PlayerHealthBar.SetIsPlayerBar();
        }

        public static void RegisterCombatListener(ICombatListener listener)
        {
            _combatListeners.Add(listener);
        }

        public static void SetCoverText(string coverText)
        {
            _coverText.text = coverText;
        }

        public static void UpdatePlayerHealth()
        {
            int currentHealth = (int) Player.HealthController.GetCurrentHealth();
            int maxHealth = (int) Player.HealthController.GetMaxHealth();
            PlayerHealthBar.SetValue(Player.HealthController.GetNormalisedHealthValue(), PlayerCanvasGroup.alpha);
            _playerHealthText.text = currentHealth + "/" + maxHealth;
        }

        public static void SetTarget(Enemy e)
        {
            CurrentTarget?.EnemyView.MarkUnselected();
            if (e == null) return;
            CurrentTarget = e;
            CurrentTarget.EnemyView.MarkSelected();
        }

        public void Update()
        {
            if (MeleeController.InMelee) return;
            _combatListeners.ForEach(l => l.UpdateCombat());
            CombatCooldowns.UpdateCooldowns();
        }

        public static void CheckPlayerFled()
        {
            Enemy nearestEnemy = UIEnemyController.NearestEnemy();
            if (nearestEnemy?.DistanceToPlayer >= VisibilityRange + 20)
            {
                //failcombat
                ExitCombat();
            }
        }

        public static void CheckEnemyFled(Enemy enemy)
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
            Player = player;
            player.EnterCombat();
            CurrentTarget = null;
            CombatCooldowns.Clear();
            _playerName.text = Player.Name;
            MenuStateMachine.ShowMenu("Combat Menu");
            _combatListeners.ForEach(l => l.EnterCombat());
        }

        public static void ExitCombat()
        {
            WorldState.UnPause();
            _combatListeners.ForEach(l => l.ExitCombat());
            Player.ExitCombat();
            if (SceneManager.GetActiveScene().name == "Combat Tester")
            {
                MenuStateMachine.ShowMenu("Retry");
            }    
            else
            {
                MenuStateMachine.ShowMenu("Game Menu");
                if (UIEnemyController.AllEnemiesGone())
                {
                    CurrentScenario.FinishCombat();
                }
            }
        }

        public static void Flee(Enemy enemy)
        {
            enemy.HasFled = true;
            CheckCombatEnd();
        }

        public static void CheckCombatEnd()
        {
            if (UIEnemyController.AllEnemiesGone())
            {
                ExitCombat();
            }
        }

        public static float DistanceBetween(float originPosition, Character target)
        {
            return Math.Abs(originPosition - target.Position.CurrentValue());
        }

        public static Enemy GetCurrentTarget()
        {
            return CurrentTarget;
        }

        public static List<Enemy> GetEnemiesBehindTarget(Enemy target)
        {
            List<Enemy> enemiesBehindTarget = new List<Enemy>();
            foreach (Enemy enemy in UIEnemyController.Enemies)
            {
                if (enemy == target) continue;
                if (enemy.Position > target.Position)
                {
                    enemiesBehindTarget.Add(enemy);
                }
            }

            return enemiesBehindTarget;
        }

        public static List<Character> GetCharactersInRange(float position, float range)
        {
            List<Character> charactersInRange = new List<Character>();
            if (Mathf.Abs(Player.Position.CurrentValue() - position) <= range)
            {
                charactersInRange.Add(Player);
            }

            foreach (Enemy enemy in UIEnemyController.Enemies)
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