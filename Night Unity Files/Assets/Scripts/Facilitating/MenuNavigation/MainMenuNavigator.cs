using System.Collections;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear.Weapons;
using Game.Global;
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
        private bool _skipping;
        private Sequence _fadeInSequence;

        private void CacheGameObjects()
        {
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
            _menuCanvasGroup.interactable = false;
            _menuCanvasGroup.blocksRaycasts = false;
            TextMeshProUGUI latinText = _latin.GetComponent<TextMeshProUGUI>();
            float finalLatinTextSize = 90;
            latinText.fontSize = finalLatinTextSize - 5f;

            TextMeshProUGUI englishText = _english.GetComponent<TextMeshProUGUI>();
            float finalEnglishTextSize = 50;
            englishText.fontSize = finalEnglishTextSize - 5f;

            _fadeInSequence = DOTween.Sequence();

#if UNITY_EDITOR
            _seenIntro = true;
#endif

            if (!_seenIntro)
            {
                _fadeInSequence.Append(_logo.DOFade(1f, 2f)); //2
                _fadeInSequence.AppendInterval(2f); //5
                _fadeInSequence.Append(_logo.DOFade(0f, 2f)); //6

                _fadeInSequence.Append(_latin.DOFade(1f, 1f)); //7
                _fadeInSequence.AppendInterval(3f); //10
                _fadeInSequence.Append(_english.DOFade(1f, 1f)); //11
                _fadeInSequence.AppendInterval(5f); //15
                _fadeInSequence.Append(_latin.DOFade(0f, 1f)); //16
                _fadeInSequence.AppendInterval(1f);

                _fadeInSequence.Insert(6, latinText.DOFontSize(finalLatinTextSize, 10));
                _fadeInSequence.Insert(10, englishText.DOFontSize(finalEnglishTextSize, 6));
                _fadeInSequence.Insert(16, _english.DOFade(0f, 1f));

                _seenIntro = true;
            }

            _fadeInSequence.AppendCallback(() => StartCoroutine(FadeInMenu()));
        }

        private IEnumerator FadeInMenu()
        {
            while (!AudioClips.Loaded()) yield return null;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_menuCanvasGroup.DOFade(1, 2f));
            sequence.AppendCallback(() =>
            {
                _menuCanvasGroup.interactable = true;
                _menuCanvasGroup.blocksRaycasts = true;
                MenuStateMachine.ShowMenu("Main Menu");
            });
        }

        public void Awake()
        {
            Application.targetFrameRate = 60;
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
            WorldState.ResetWorld(true, (num - 1) * 10);
            if (num > 2) CharacterManager.AddCharacter(CharacterManager.GenerateRandomCharacter(CharacterClass.Protector));
            if (num > 4) CharacterManager.AddCharacter(CharacterManager.GenerateRandomCharacter());
            SaveController.ManualSave();
            GameController.StartGame(true);
            _skipping = true;
        }

        public void StartNewGame()
        {
            _fadeInSequence.Complete();
            MenuStateMachine.ShowMenu(SaveController.SaveExists() ? "Overwrite Save Warning" : "Difficulty");
        }

        public void ShowMenu(Menu menu)
        {
            _fadeInSequence.Complete();
            MenuStateMachine.ShowMenu(menu.name);
        }
    }
}