using DG.Tweening;
using Facilitating.Persistence;
using Game.Gear.Weapons;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour, IInputListener
    {
        private CanvasGroup _menuCanvasGroup;
        private static bool _shownSplashScreen;
        private CanvasGroup _logo, _latin, _english;
        private Sequence _fadeInSequence;
        private static bool _seenIntro;
        private GameController _gameController;

        private void CacheGameObjects()
        {
            _gameController = gameObject.GetComponent<GameController>();
            _menuCanvasGroup = gameObject.FindChildWithName<CanvasGroup>("Menu Canvas Group");
            _menuCanvasGroup.alpha = 0f;
            _latin = gameObject.FindChildWithName<CanvasGroup>("Latin");
            _english = gameObject.FindChildWithName<CanvasGroup>("English");
            _logo = gameObject.FindChildWithName<CanvasGroup>("Logo");
            _english.alpha = 0f;
            _latin.alpha = 0f;
            _logo.alpha = 0f;
        }

        private void CreateFadeInSequence()
        {
            _fadeInSequence = DOTween.Sequence();
            if (!_seenIntro)
            {
                _fadeInSequence.AppendInterval(1f); //1
                _fadeInSequence.Append(_logo.DOFade(1f, 1f)); //2
                _fadeInSequence.AppendInterval(3f); //5
                _fadeInSequence.Append(_logo.DOFade(0f, 1f)); //6
                _fadeInSequence.Append(_latin.DOFade(1f, 1f)); //7
                _fadeInSequence.AppendInterval(3f); //10
                _fadeInSequence.Append(_english.DOFade(1f, 1f)); //11
                _fadeInSequence.AppendInterval(6f); //17
                _fadeInSequence.Append(_latin.DOFade(0f, 1f)); //18
                _fadeInSequence.Insert(17, _english.DOFade(0f, 1f));
                _fadeInSequence.AppendInterval(2f);
                _seenIntro = true;
            }

            _fadeInSequence.Join(_menuCanvasGroup.DOFade(1, 2f));
            _fadeInSequence.AppendCallback(() => MenuStateMachine.ShowMenu("Main Menu"));
        }

        public void Awake()
        {
            Cursor.visible = false;
            InputHandler.RegisterInputListener(this);
            SaveController.LoadSettings();
            CacheGameObjects();
            CreateFadeInSequence();
            CheckForExistingSave();
            WeaponGenerationTester.Test();
        }

        private void CheckForExistingSave()
        {
            if (SaveController.SaveExists()) return;
            GameObject menuContainer = gameObject.FindChildWithName("Main Menu");
            GameObject continueButton = menuContainer.FindChildWithName("Continue");
            continueButton.SetActive(false);
            EnhancedButton newGameButton = menuContainer.FindChildWithName<EnhancedButton>("New Game");
            EnhancedButton optionsButton = menuContainer.FindChildWithName<EnhancedButton>("Options");
            newGameButton.SetDownNavigation(optionsButton);
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            InputHandler.UnregisterInputListener(this);
            _fadeInSequence.Complete(true);
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void StartNewGame()
        {
            if (SaveController.SaveExists())
                MenuStateMachine.ShowMenu("Overwrite Save Warning");
            else
                _gameController.ClearSaveAndLoad();
        }

        public void ShowMenu(Menu menu)
        {
            MenuStateMachine.ShowMenu(menu.name);
        }
    }
}