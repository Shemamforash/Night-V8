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
        private AudioSource _audioSource;
        private static string _sceneToLoad;
        private const float DefaultFadeTime = 0.5f;

        public void Awake()
        {
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
            VolumeController.SetModifiedVolume(0f);
            float fadeInTime = SceneManager.GetActiveScene().name == "Combat" ? 3f : DefaultFadeTime;
            DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 1f, fadeInTime).SetUpdate(UpdateType.Normal, true);
            ScreenFaderController.SetAlpha(1);
            ScreenFaderController.FadeOut(fadeInTime);
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
            if (_audioSource != null) _audioSource.DOFade(1, DefaultFadeTime);
            VolumeController.SetModifiedVolume(1f);
            DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 0f, DefaultFadeTime).SetUpdate(UpdateType.Normal, true);
            ScreenFaderController.FadeIn(DefaultFadeTime);
            yield return new WaitForSeconds(DefaultFadeTime);
            StartCoroutine(LoadNextScene());
        }

        public static void GoToGameOverScene()
        {
            ChangeScene("Game Over");
        }

        public static void GoToStoryScene()
        {
            ChangeScene("Story");
        }

        public static void GoToCombatScene()
        {
            ChangeScene("Combat"); // true);
        }

        public static void GoToMainMenuScene()
        {
            ChangeScene("Menu");
        }

        public static void GoToCreditsScene()
        {
            ChangeScene("Credits");
        }

        public static void GoToGameScene()
        {
            ChangeScene("Game");
        }

        private static void ChangeScene(string sceneName)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName));
        }
    }
}