﻿using System.Collections;
using Game.Global;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Facilitating.UI
{
    public class DayChangeSequence : Menu
    {
        private CanvasGroup _menuScreen, _thisCanvasGroup;
        public float TimeBetweenLines = 1f;
        public float TimeToRead = 3f;
        private static DayChangeSequence _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;
            _thisCanvasGroup = GetComponent<CanvasGroup>();
            _menuScreen = GameObject.Find("Game Menu").GetComponent<CanvasGroup>();
        }

        public static DayChangeSequence Instance()
        {
            return _instance;
        }
        
        public void ChangeDay()
        {
            WorldState.Pause();
            _menuScreen.interactable = false;
            _menuScreen.alpha = 0;
            _thisCanvasGroup.interactable = true;
            _thisCanvasGroup.alpha = 1;
            StartCoroutine(ShowLines());
        }

        private IEnumerator WaitToReadText()
        {
            float currentTime = 0;
            while (currentTime < TimeToRead)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }

            TransitionEnd();
        }

        private void TransitionEnd()
        {
            _menuScreen.alpha = 1;
            _thisCanvasGroup.alpha = 0;
            WorldState.Resume();
            _menuScreen.interactable = true;
            _thisCanvasGroup.interactable = false;
        }

        private IEnumerator ShowLines()
        {
            float currentTime = 0f;
            int currentChild = 0;
            for (int i = 0; i < transform.childCount; ++i) transform.GetChild(i).gameObject.SetActive(false);
            while (currentTime < TimeBetweenLines)
            {
                currentTime += Time.deltaTime;
                if (currentTime > TimeBetweenLines && currentChild < transform.childCount)
                {
                    transform.GetChild(currentChild).gameObject.SetActive(true);
                    ++currentChild;
                    currentTime = 0f;
                }

                yield return null;
            }

            yield return StartCoroutine(nameof(WaitToReadText));
        }
    }
}