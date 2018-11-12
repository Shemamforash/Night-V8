using System.Collections;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour
    {
        private CanvasGroup _menuCanvasGroup;
        private static bool _shownSplashScreen;
        private CanvasGroup _logo, _latin, _english;
        private static bool _seenIntro;
        private GameController _gameController;
        private bool _skipping;

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
            TextMeshProUGUI latinText = _latin.GetComponent<TextMeshProUGUI>();
            float finalLatinTextSize = 90;
            latinText.fontSize = finalLatinTextSize - 5f;

            TextMeshProUGUI englishText = _english.GetComponent<TextMeshProUGUI>();
            float finalEnglishTextSize = 50;
            englishText.fontSize = finalEnglishTextSize - 5f;

            Sequence fadeInSequence = DOTween.Sequence();
            
#if UNITY_EDITOR
            _seenIntro = true;
#endif
            
            if (!_seenIntro)
            {
                fadeInSequence.Append(_logo.DOFade(1f, 2f)); //2
                fadeInSequence.AppendInterval(2f); //5
                fadeInSequence.Append(_logo.DOFade(0f, 2f)); //6

                fadeInSequence.Append(_latin.DOFade(1f, 1f)); //7
                fadeInSequence.AppendInterval(3f); //10
                fadeInSequence.Append(_english.DOFade(1f, 1f)); //11
                fadeInSequence.AppendInterval(5f); //15
                fadeInSequence.Append(_latin.DOFade(0f, 1f)); //16
                fadeInSequence.AppendInterval(1f);

                fadeInSequence.Insert(6, latinText.DOFontSize(finalLatinTextSize, 10));
                fadeInSequence.Insert(10, englishText.DOFontSize(finalEnglishTextSize, 6));
                fadeInSequence.Insert(16, _english.DOFade(0f, 1f));

                _seenIntro = true;
            }

            fadeInSequence.AppendCallback(() => StartCoroutine(FadeInMenu()));
        }

        private IEnumerator FadeInMenu()
        {
            while (!AudioClips.Loaded()) yield return null;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_menuCanvasGroup.DOFade(1, 2f));
            sequence.AppendCallback(() => MenuStateMachine.ShowMenu("Main Menu"));
        }

        public void Awake()
        {
            _skipping = false;
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


        public void Update()
        {
            if (_skipping) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) SkipToPoint(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SkipToPoint(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SkipToPoint(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SkipToPoint(4);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) SkipToPoint(5);
        }

        private void SkipToPoint(int num)
        {
            SaveController.ClearSave();
            WorldState.ResetWorld(num, (num - 1) * 10);
            if (num > 2) CharacterManager.AddCharacter(CharacterManager.GenerateRandomCharacter());
            if (num > 4) CharacterManager.AddCharacter(CharacterManager.GenerateRandomCharacter());
            SaveController.SaveGame();
            _gameController.StartGame(true);
            _skipping = true;
        }

        public void StartNewGame()
        {
            MenuStateMachine.ShowMenu(SaveController.SaveExists() ? "Overwrite Save Warning" : "Tutorial Choice");
        }

        public void ShowMenu(Menu menu)
        {
            MenuStateMachine.ShowMenu(menu.name);
        }
    }
}