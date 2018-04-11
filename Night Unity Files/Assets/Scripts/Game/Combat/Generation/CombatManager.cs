using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Exploration.Region;
using Game.Global;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Combat.Generation
{
    public class CombatManager : Menu
    {
        private const float _fadeTime = 1f;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        public static UIEnemyController EnemyController;
        public static Region CurrentRegion;
        private static bool _inMelee;
        public static int VisibilityRange;
        public static PlayerCombat Player;

        private static bool _failed;

        public void Awake()
        {
            Player = GameObject.Find("Player").GetComponent<PlayerCombat>();
            EnemyController = Helper.FindChildWithName<UIEnemyController>(gameObject, "Enemies");
        }

        public void Start()
        {
            EnterCombat();
        }

        public void Update()
        {
            CombatCooldowns.UpdateCooldowns();
            if (_failed)
            {
                SucceedCombat();
                return;
            }

            if (CurrentRegion.Enemies().Any(e => !e.IsDead)) return;
            SucceedCombat();
        }

        private void EnterCombat()
        {
            _failed = false;
            WorldState.Pause();
            VisibilityRange = 200;
            CurrentRegion = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().Region;

            List<AreaGenerator.Shape> barriers = AreaGenerator.Instance().GenerateArea();

            PathingGrid.Instance().GenerateGrid(barriers);

//            VisibilityRange = (int) (100 * WeatherManager.Instance().CurrentWeather().GetVisibility());

            Player.Initialise();
            CombatCooldowns.Clear();
            EnemyController.EnterCombat();
        }

        private void SucceedCombat()
        {
            if (SceneManager.GetActiveScene().name == "Combat")
            {
                if (SceneChanger.Fading) return;
                SceneChanger.ChangeScene("Combat");
            }
            else
            {
                MenuStateMachine.ShowMenu("Game Menu");
            }

            ExitCombat();
        }

        public static void FailCombat()
        {
            _failed = true;
//            MenuStateMachine.ShowMenu(SceneManager.GetActiveScene().name == "Combat" ? "Minigame Menu" : "Game Menu");
//            ExitCombat();
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

        public static List<EnemyBehaviour> GetEnemiesBehindTarget(EnemyBehaviour target)
        {
            List<EnemyBehaviour> enemiesBehindTarget = new List<EnemyBehaviour>();
            return enemiesBehindTarget;
        }

        public static List<CharacterCombat> GetCharactersInRange(Vector2 position, float range)
        {
            List<CharacterCombat> charactersInRange = new List<CharacterCombat>();
            if (Vector2.Distance(Player.transform.position, position) <= range) charactersInRange.Add(Player);

            foreach (EnemyBehaviour enemy in UIEnemyController.Enemies)
                if (Vector2.Distance(enemy.transform.position, position) <= range)
                    charactersInRange.Add(enemy);

            return charactersInRange;
        }
    }
}