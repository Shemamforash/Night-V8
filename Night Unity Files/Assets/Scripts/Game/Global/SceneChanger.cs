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
        private static string _sceneToLoad;
        private const float DefaultFadeTime = 0.5f;

        public void Awake()
        {
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
            _fader = GameObject.Find("Screen Fader").GetComponent<CanvasGroup>();
            VolumeController.SetModifiedVolume(0f);
            float fadeInTime = SceneManager.GetActiveScene().name == "Combat" ? 3f : DefaultFadeTime;
            DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 1f, fadeInTime);
            Sequence sequence = DOTween.Sequence();
            _fader.alpha = 1;
            sequence.Append(_fader.DOFade(0, fadeInTime));
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

        private IEnumerator FadeOut(string sceneName, bool goToLoadingScene = true)
        {
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
            ChangeScene("Game Over");
        }

        public static void GoToMapScene()
        {
            ChangeScene("Map");
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

        private static void ChangeScene(string sceneName, bool goToLoadingScene = false)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName, goToLoadingScene));
        }
    }
}