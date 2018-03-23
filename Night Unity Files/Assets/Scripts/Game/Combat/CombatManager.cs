using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Facilitating.UIControllers;
using Game.Characters.Player;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
        public Image _screenFade;
        private const float _fadeTime = 1f;

        public void Awake()
        {
            Player = GameObject.Find("Player").GetComponent<PlayerCombat>();
            CombatCanvas = Helper.FindChildWithName<CanvasGroup>(gameObject, "Combat Canvas");
            EnemyController = Helper.FindChildWithName<UIEnemyController>(gameObject, "Enemies");
            _screenFade = GameObject.Find("Screen Fade").GetComponent<Image>();
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            float timeElapsed = 0f;
            while (timeElapsed < _fadeTime)
            {
                float alpha = 1 - timeElapsed / _fadeTime;
                _screenFade.color = new Color(0, 0, 0, alpha);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            _screenFade.color = UiAppearanceController.InvisibleColour;
        }

        public void Update()
        {
            CombatCooldowns.UpdateCooldowns();
            if (_failed)
            {
                SucceedCombat();
                return;
            }
            if (CurrentScenario.Enemies().Any(e => !e.IsDead)) return;
            SucceedCombat();
        }

        public static void EnterCombat(Player player, CombatScenario scenario)
        {
            _failed = false;
            WorldState.Pause();
            VisibilityRange = 200;
            CurrentScenario = scenario;
            MenuStateMachine.ShowMenu("Combat Menu");

            List<AreaGenerator.Shape> barriers = AreaGenerator.GenerateArea();

            PathingGrid.Instance().GenerateGrid(barriers);

//            VisibilityRange = (int) (100 * WeatherManager.Instance().CurrentWeather().GetVisibility());

            Player.Initialise(player);
            CombatCooldowns.Clear();
            EnemyController.EnterCombat();
        }

        private bool _loadingNextSene = false;
        private static bool _failed;

        private IEnumerator FadeOut(AsyncOperation sceneLoaded)
        {
            float timeElapsed = 0f;
            sceneLoaded.allowSceneActivation = false;
            _loadingNextSene = true;
            float alpha = 0;
            while (!sceneLoaded.isDone && alpha != 1)
            {
                alpha = timeElapsed / (_fadeTime * 4);
                if (alpha > 1) alpha = 1;
                _screenFade.color = new Color(0, 0, 0, alpha);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            sceneLoaded.allowSceneActivation = true;
        }

        private void SucceedCombat()
        {
            if (SceneManager.GetActiveScene().name == "Combat Tester")
            {
                if (_loadingNextSene) return;
                AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync("Combat Tester");
                StartCoroutine(FadeOut(sceneLoaded));
//                MenuStateMachine.ShowMenu("Next Level");
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
            _failed = true;
//            MenuStateMachine.ShowMenu(SceneManager.GetActiveScene().name == "Combat Tester" ? "Minigame Menu" : "Game Menu");
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
            if (Vector2.Distance(Player.transform.position, position) <= range)
            {
                charactersInRange.Add(Player);
            }

            foreach (EnemyBehaviour enemy in UIEnemyController.Enemies)
            {
                if (Vector2.Distance(enemy.transform.position, position) <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }
    }
}