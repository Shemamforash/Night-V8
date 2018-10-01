using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Global
{
    public class SceneChanger : MonoBehaviour
    {
        private static CanvasGroup _fader;
        private static SceneChanger _instance;
        private AudioSource _audioSource;
        private static float _fadeInTime = DefaultFadeTime;
        private static string _sceneToLoad;
        private const float DefaultFadeTime = 0.5f;

        public void Awake()
        {
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
            _fader = GameObject.Find("Screen Fader").GetComponent<CanvasGroup>();
            VolumeController.SetModifiedVolume(0f);
            DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 1f, _fadeInTime);
            Sequence sequence = DOTween.Sequence();
            _fader.alpha = 1;
            sequence.Append(_fader.DOFade(0, _fadeInTime));
            sequence.AppendCallback(() => StartCoroutine(LoadNextScene()));
            if (SceneManager.GetActiveScene().name != "Game") return;
            sequence.InsertCallback(0.1f, WorldState.UnPause);
        }

        private IEnumerator LoadNextScene()
        {
            if (_sceneToLoad == null) yield break;
            AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync(_sceneToLoad);
            _sceneToLoad = null;
            sceneLoaded.allowSceneActivation = false;
            while (sceneLoaded.progress != 0.9f) yield return null;
            sceneLoaded.allowSceneActivation = true;
            ButtonClickListener.SuppressClick();
        }

        private IEnumerator FadeOut(string sceneName, float fadeTime, bool goToLoadingScene = true)
        {
            _fadeInTime = fadeTime;
            _sceneToLoad = sceneName;
            if (_audioSource != null) _audioSource.DOFade(1, DefaultFadeTime);
            VolumeController.SetModifiedVolume(1f);
            DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 0f, DefaultFadeTime);
            yield return _fader.DOFade(1, DefaultFadeTime).WaitForCompletion();
            TryGoToLoadingScene(goToLoadingScene);
        }

        private void TryGoToLoadingScene(bool goToLoadingScene)
        {
            if (goToLoadingScene)
            {
                SceneManager.LoadScene("Loading");
                return;
            }

            StartCoroutine(LoadNextScene());
        }

        public static void GoToGameOverScene()
        {
            ChangeScene("Game Over", DefaultFadeTime);
        }

        public static void GoToMapScene()
        {
            ChangeScene("Map", DefaultFadeTime);
        }

        public static void GoToStoryScene()
        {
            ChangeScene("Story", DefaultFadeTime);
        }

        public static void GoToCombatScene()
        {
            ChangeScene("Combat", 5f, true);
        }

        public static void GoToMainMenuScene()
        {
            ChangeScene("Menu", DefaultFadeTime);
        }

        public static void GoToCreditsScene()
        {
            ChangeScene("Credits", DefaultFadeTime);
        }

        public static void GoToGameScene()
        {
            ChangeScene("Game", DefaultFadeTime);
        }

        private static void ChangeScene(string sceneName, float fadeTime, bool goToLoadingScene = false)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName, fadeTime, goToLoadingScene));
        }
    }
}