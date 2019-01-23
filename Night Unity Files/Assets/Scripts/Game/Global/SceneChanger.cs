using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Facilitating.UIControllers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Game.Global
{
    public class SceneChanger : MonoBehaviour
    {
        private static SceneChanger _instance;
        private static string _sceneToLoad;
        private static bool _fadeInAudio;
        private Tweener _volumeTweener;
        private static bool _changingScene;
        private static Action _onSceneEnd;
        private const float DefaultFadeTime = 0.5f;

        public void Awake()
        {
            _changingScene = false;
            _instance = this;
            float fadeInTime = SceneManager.GetActiveScene().name == "Combat" ? 3f : DefaultFadeTime;
            ScreenFaderController.SetAlpha(1);
            ScreenFaderController.FadeOut(fadeInTime);
            if (_fadeInAudio)
            {
                VolumeController.SetModifiedVolume(0f);
                _volumeTweener = DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 1f, DefaultFadeTime).SetUpdate(UpdateType.Normal, true);
                _fadeInAudio = false;
            }

            if (SceneManager.GetActiveScene().name == "Game") WorldState.Resume();
        }

        private IEnumerator LoadNextScene()
        {
            Assert.IsNotNull(_sceneToLoad);
            AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync(_sceneToLoad);
            sceneLoaded.allowSceneActivation = false;
            while (sceneLoaded.progress != 0.9f) yield return null;
            sceneLoaded.allowSceneActivation = true;
            ButtonClickListener.SuppressClick();
        }

        private IEnumerator FadeOut(string sceneName)
        {
            _changingScene = true;
            _sceneToLoad = sceneName;
            ScreenFaderController.FadeIn(DefaultFadeTime);
            if (_fadeInAudio)
            {
                _volumeTweener?.Kill();
                DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 0f, DefaultFadeTime).SetUpdate(UpdateType.Normal, true);
            }

            Time.timeScale = 1f;
            yield return new WaitForSeconds(DefaultFadeTime);
            _onSceneEnd?.Invoke();
            _onSceneEnd = null;
            StartCoroutine(LoadNextScene());
        }

        public static void GoToGameOverScene(DeathReason deathReason)
        {
            UIDeathController.DeathReason = deathReason;
            FadeInAudio();
            ChangeScene("Game Over");
        }

        public static void GoToStoryScene()
        {
            FadeInAudio();
            ChangeScene("Story");
        }

        public static void GoToCombatScene(Action onSceneEnd = null)
        {
            _onSceneEnd = onSceneEnd;
            ChangeScene("Combat");
        }

        public static void GoToMainMenuScene()
        {
            FadeInAudio();
            ChangeScene("Menu");
        }

        public static void GoToCreditsScene()
        {
            FadeInAudio();
            ChangeScene("Credits");
        }

        public static void GoToGameScene()
        {
            ChangeScene("Game");
        }

        public static void FadeInAudio()
        {
            _fadeInAudio = true;
        }

        private static void ChangeScene(string sceneName)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName));
        }

        public static bool ChangingScene()
        {
            return _changingScene;
        }
    }
}