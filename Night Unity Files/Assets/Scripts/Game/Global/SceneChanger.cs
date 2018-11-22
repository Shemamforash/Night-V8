using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Global
{
    public class SceneChanger : MonoBehaviour
    {
        private static SceneChanger _instance;
        private static string _sceneToLoad;
        private static bool _fadeInAudio;
        private const float DefaultFadeTime = 0.5f;

        public void Awake()
        {
            _instance = this;
            float fadeInTime = SceneManager.GetActiveScene().name == "Combat" ? 3f : DefaultFadeTime;
            ScreenFaderController.SetAlpha(1);
            ScreenFaderController.FadeOut(fadeInTime);
            if (_fadeInAudio)
            {
                VolumeController.SetModifiedVolume(0f);
                DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 1f, DefaultFadeTime).SetUpdate(UpdateType.Normal, true);
                _fadeInAudio = false;
            }

            if (SceneManager.GetActiveScene().name == "Game") WorldState.UnPause();
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
            _sceneToLoad = sceneName;
            ScreenFaderController.FadeIn(DefaultFadeTime);
            if (_fadeInAudio)
            {
                DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 0f, DefaultFadeTime).SetUpdate(UpdateType.Normal, true);
            }

            yield return new WaitForSeconds(DefaultFadeTime);
            StartCoroutine(LoadNextScene());
        }

        public static void GoToGameOverScene()
        {
            FadeInAudio();
            ChangeScene("Game Over");
        }

        public static void GoToStoryScene()
        {
            FadeInAudio();
            ChangeScene("Story");
        }

        public static void GoToCombatScene()
        {
            ChangeScene("Combat"); // true);
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
    }
}