﻿using System.Collections;
using DG.Tweening;
using Extensions;
using Facilitating.Persistence;
using Game.Gear.Weapons;
using Game.Global;

using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

namespace Facilitating.MenuNavigation
{
	public class MainMenuNavigator : MonoBehaviour
	{
		private static bool        _shownSplashScreen;
		private static bool        _seenIntro;
		private        Sequence    _fadeInSequence;
		private        CanvasGroup _logo, _latin, _english, _loading;
		private        CanvasGroup _menuCanvasGroup;

		private void CacheGameObjects()
		{
			_menuCanvasGroup       = gameObject.FindChildWithName<CanvasGroup>("Menu Canvas Group");
			_menuCanvasGroup.alpha = 0f;
			_latin                 = gameObject.FindChildWithName<CanvasGroup>("Latin");
			_english               = gameObject.FindChildWithName<CanvasGroup>("English");
			_logo                  = gameObject.FindChildWithName<CanvasGroup>("Logo");
			_loading               = gameObject.FindChildWithName<CanvasGroup>("Loading");
			_loading.alpha         = 0f;
			_english.alpha         = 0f;
			_latin.alpha           = 0f;
			_logo.alpha            = 0f;
		}

		private void CreateFadeInSequence()
		{
			_menuCanvasGroup.interactable   = false;
			_menuCanvasGroup.blocksRaycasts = false;
			TextMeshProUGUI latinText          = _latin.GetComponent<TextMeshProUGUI>();
			float           finalLatinTextSize = 90;
			latinText.fontSize = finalLatinTextSize - 5f;

			TextMeshProUGUI englishText          = _english.GetComponent<TextMeshProUGUI>();
			float           finalEnglishTextSize = 50;
			englishText.fontSize = finalEnglishTextSize - 5f;

			_fadeInSequence = DOTween.Sequence();

#if UNITY_EDITOR
			_seenIntro = true;
#endif

			if (!_seenIntro)
			{
				_loading.DOFade(0.5f, 2f);
				_fadeInSequence.Append(_logo.DOFade(1f, 2f)); //2
				_fadeInSequence.AppendInterval(2f);           //5
				_fadeInSequence.Append(_logo.DOFade(0f, 2f)); //6

				_fadeInSequence.Append(_latin.DOFade(1f, 1f));   //7
				_fadeInSequence.AppendInterval(3f);              //10
				_fadeInSequence.Append(_english.DOFade(1f, 1f)); //11
				_fadeInSequence.AppendInterval(5f);              //15
				_fadeInSequence.Append(_latin.DOFade(0f, 1f));   //16
				_fadeInSequence.AppendInterval(1f);

				_fadeInSequence.Insert(6,  latinText.DOFontSize(finalLatinTextSize, 10));
				_fadeInSequence.Insert(10, englishText.DOFontSize(finalEnglishTextSize, 6));
				_fadeInSequence.Insert(16, _english.DOFade(0f, 1f));
				_fadeInSequence.Insert(16, _loading.DOFade(0f, 1f));

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
				_menuCanvasGroup.interactable   = true;
				_menuCanvasGroup.blocksRaycasts = true;
				MenuStateMachine.ShowMenu("Main Menu");
			});
		}

		public void Awake()
		{
			Application.targetFrameRate = 60;
			SaveController.LoadSettings();
			CacheGameObjects();
			CreateFadeInSequence();
			CheckForExistingSave();
#if UNITY_EDITOR
//			WeaponGenerationTester.Test();
#endif
		}

		private void CheckForExistingSave()
		{
			if (SaveController.SaveExists()) return;
			GameObject menuContainer  = gameObject.FindChildWithName("Main Menu");
			GameObject continueButton = menuContainer.FindChildWithName("Continue");
			continueButton.SetActive(false);
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