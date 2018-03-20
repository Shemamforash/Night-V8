using System.Collections.Generic;
using System.Linq;
using Facilitating.UIControllers;
using Game.Characters.Player;
using Game.Combat.CharacterUi;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CanvasGroup CombatCanvas;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        public static UIEnemyController EnemyController;
        public static CombatScenario CurrentScenario;
        private static bool _inMelee;
        public static int VisibilityRange;
        public static PlayerCombat Player;

        public void Awake()
        {
            Player = GameObject.Find("Player").GetComponent<PlayerCombat>();
            CombatCanvas = Helper.FindChildWithName<CanvasGroup>(gameObject, "Combat Canvas");
            EnemyController = Helper.FindChildWithName<UIEnemyController>(gameObject, "Enemies");
        }

        public void Update()
        {
            if (MeleeController.InMelee) return;
            CombatCooldowns.UpdateCooldowns();
            if (CurrentScenario.Enemies().Any(e => !e.IsDead)) return;
            SucceedCombat();
        }

        public static void EnterCombat(Player player, CombatScenario scenario)
        {
            WorldState.Pause();
            VisibilityRange = 200;
            CurrentScenario = scenario;
            MenuStateMachine.ShowMenu("Combat Menu");
            List<AreaGenerator.Shape> barriers = AreaGenerator.GenerateArea();
            PathingGrid.Instance().SetShapes(barriers);
//            VisibilityRange = (int) (100 * WeatherManager.Instance().CurrentWeather().GetVisibility());
            
            Player.Initialise(player);
            CombatCooldowns.Clear();
            EnemyController.EnterCombat();
        }

        private static void SucceedCombat()
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
//            MenuStateMachine.ShowMenu(SceneManager.GetActiveScene().name == "Combat Tester" ? "Minigame Menu" : "Game Menu");
            ExitCombat();
        }

        private static void ExitCombat()
        {
            WorldState.UnPause();
            Player.ExitCombat();
            EnemyController.ExitCombat();
        }

//        public static float DistanceBetween(float originPosition, CharacterCombat target)
//        {
//            return Math.Abs(originPosition - target.Position.CurrentValue());
//        }

        public static List<DetailedEnemyCombat> GetEnemiesBehindTarget(DetailedEnemyCombat target)
        {
            List<DetailedEnemyCombat> enemiesBehindTarget = new List<DetailedEnemyCombat>();
            return enemiesBehindTarget;
        }

        public static List<CharacterCombat> GetCharactersInRange(Vector2 position, float range)
        {
            List<CharacterCombat> charactersInRange = new List<CharacterCombat>();
            if (Vector2.Distance(Player.CharacterController.Position(), position) <= range)
            {
                charactersInRange.Add(Player);
            }

            foreach (DetailedEnemyCombat enemy in UIEnemyController.Enemies)
            {
                if (Vector2.Distance(enemy.CharacterController.Position(), position) <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }
    }
}