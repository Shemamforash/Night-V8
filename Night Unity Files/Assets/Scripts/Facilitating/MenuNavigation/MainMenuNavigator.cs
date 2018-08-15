using DG.Tweening;
using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour, IInputListener
    {
        private CanvasGroup _menuCanvasGroup;
        private static bool _shownSplashScreen;
        private Image _logo, _fireBackground, _loadingIcon;
        private ParticleSystem _fireParticles;
        private Sequence _fadeInSequence;
        private bool _starting;
        private static bool _seenIntro;

        private void CacheGameobjects()
        {
            _menuCanvasGroup = gameObject.FindChildWithName<CanvasGroup>("Menu Canvas Group");
            _logo = gameObject.FindChildWithName<Image>("Logo");
            _fireBackground = gameObject.FindChildWithName<Image>("Fire Image");
            _loadingIcon = gameObject.FindChildWithName<Image>("Loading Icon");
            _fireParticles = GameObject.Find("Fire").GetComponent<ParticleSystem>();
        }

        private void ResetAppearance()
        {
            _loadingIcon.fillAmount = 0;
            _menuCanvasGroup.alpha = 0f;
            _logo.color = UiAppearanceController.InvisibleColour;
            _fireBackground.color = UiAppearanceController.InvisibleColour;
        }

        private void CreateFadeInSequence()
        {
            _fadeInSequence = DOTween.Sequence();
            if (!_seenIntro)
            {
                _fadeInSequence.AppendInterval(1f);
                _fadeInSequence.Append(_logo.DOColor(Color.white, 1f));
                _fadeInSequence.AppendInterval(2f);
                _fadeInSequence.Append(_logo.DOColor(UiAppearanceController.InvisibleColour, 1f));
                _seenIntro = true;
            }

            _fadeInSequence.AppendCallback(() => _fireParticles.Play());
            _fadeInSequence.Append(_fireBackground.DOColor(new Color(1f, 0.4f, 0f, 1f), 1));
            _fadeInSequence.Join(_menuCanvasGroup.DOFade(1, 2f));
            _fadeInSequence.AppendCallback(() => MenuStateMachine.ShowMenu("Main Menu"));
        }
        
        public void Awake()
        {
            Cursor.visible = false;
            InputHandler.RegisterInputListener(this);
            CacheGameobjects();
            ResetAppearance();
            CreateFadeInSequence();
            _starting = false;
            ContinueGame();
//            ClearSaveAndLoad();
//            EditorApplication.isPlaying = false;
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
            StartGame();
        }

        private void StartGame()
        {
            _starting = true;
            SceneChanger.ChangeScene("Game", true, f => _loadingIcon.fillAmount = f);
            InputHandler.SetCurrentListener(null);
        }

        public void ContinueGame()
        {
            if (SaveController.SaveExists() && !_starting)
            {
                SaveController.LoadGame();
                StartGame();
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
    }
}