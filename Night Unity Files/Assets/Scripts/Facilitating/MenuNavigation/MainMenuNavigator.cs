using DG.Tweening;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour, IInputListener
    {
        private CanvasGroup _menuCanvasGroup;
        private static bool _shownSplashScreen;
        private Image _loadingIcon;
        private CanvasGroup _logo;
        private Sequence _fadeInSequence;
        private bool _starting;
        private static bool _seenIntro;
        public bool Load;

        private void CacheGameobjects()
        {
            _menuCanvasGroup = gameObject.FindChildWithName<CanvasGroup>("Menu Canvas Group");
            _logo = gameObject.FindChildWithName<CanvasGroup>("Logo");
            _loadingIcon = gameObject.FindChildWithName<Image>("Loading Icon");
        }

        private void ResetAppearance()
        {
            _loadingIcon.fillAmount = 0;
            _menuCanvasGroup.alpha = 0f;
            _logo.alpha = 0f;
        }

        private void CreateFadeInSequence()
        {
            _fadeInSequence = DOTween.Sequence();
            if (!_seenIntro)
            {
                _fadeInSequence.AppendInterval(1f);
                _fadeInSequence.Append(_logo.DOFade(1f, 1f));
                _fadeInSequence.AppendInterval(2f);
                _fadeInSequence.Append(_logo.DOFade(0f, 1f));
                _seenIntro = true;
            }

            _fadeInSequence.Join(_menuCanvasGroup.DOFade(1, 2f));
            _fadeInSequence.AppendCallback(() => MenuStateMachine.ShowMenu("Main Menu"));
        }

        public void Awake()
        {
            SaveController.LoadSettings();
            Cursor.visible = false;
            InputHandler.RegisterInputListener(this);
            CacheGameobjects();
            ResetAppearance();
            CreateFadeInSequence();
            _starting = false;
            return;
            if (Load)
            {
                ContinueGame();
                return;
            }

            ClearSaveAndLoad();
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

        public void CloseGame()
        {
            Application.Quit();
        }

        public void StartNewGame()
        {
            if (SaveController.SaveExists())
                MenuStateMachine.ShowMenu("Overwrite Save Warning");
            else
                ClearSaveAndLoad();
        }

        private void ClearSaveAndLoad()
        {
            if (_starting) return;
            _starting = true;
            SaveController.ClearSave();
            WorldState.ResetWorld();
            SaveController.SaveGame();
            StartGame(true);
        }

        private void StartGame(bool newGame, Travel travel = null)
        {
            _starting = true;
            InputHandler.SetCurrentListener(null);
            if (newGame) StoryController.ShowText(JournalEntry.GetStoryText(1), false);
            else if(travel != null) travel.Enter();
            else SceneChanger.GoToGameScene(f => _loadingIcon.fillAmount = f);
        }

        public void ContinueGame()
        {
            if (SaveController.SaveExists() && !_starting)
            {
                Travel travel = SaveController.LoadGame();
                StartGame(false, travel);
            }
            else
            {
                MenuStateMachine.ShowMenu("No Save Warning");
            }
        }

        public void ShowMenu(Menu menu)
        {
            MenuStateMachine.ShowMenu(menu.name);
        }

        public void QuitToMenu()
        {
            SaveController.QuickSave();
        }

        public void QuitToDesktop()
        {
            SaveController.QuickSave();
            Application.Quit();
        }
    }
}