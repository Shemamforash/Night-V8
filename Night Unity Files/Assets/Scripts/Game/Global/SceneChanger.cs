using System;
using System.Collections;
using DG.Tweening;
using Facilitating.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Global
{
    public class SceneChanger : MonoBehaviour
    {
        private static CanvasGroup _fader;
        private static SceneChanger _instance;
        private AudioSource _audioSource;
        private static float _fadeInTime = DefaultFadeTime;
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
            if (SceneManager.GetActiveScene().name != "Game") return;
            sequence.InsertCallback(0.1f, WorldState.UnPause);
        }

        private IEnumerator FadeOut(string sceneName, float fadeTime, Action<float> loadProgressAction)
        {
            _fadeInTime = fadeTime;
            if (_audioSource != null) _audioSource.DOFade(1, DefaultFadeTime);
            VolumeController.SetModifiedVolume(1f);
            DOTween.To(VolumeController.Volume, VolumeController.SetModifiedVolume, 0f, DefaultFadeTime);
            yield return _fader.DOFade(1, DefaultFadeTime).WaitForCompletion();

            AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync(sceneName);
            sceneLoaded.allowSceneActivation = false;
            loadProgressAction?.Invoke(sceneLoaded.progress);
            while (sceneLoaded.progress != 0.9f)
            {
                yield return null;
                loadProgressAction?.Invoke(sceneLoaded.progress);
            }

            loadProgressAction?.Invoke(1);
            sceneLoaded.allowSceneActivation = true;
            ButtonClickListener.SuppressClick();
        }

        public static void GoToGameOverScene()
        {
            ChangeScene("Game Over", DefaultFadeTime);
        }

        public static void GoToMapScene()
        {
            Debug.Log("map");
            ChangeScene("Map", DefaultFadeTime);
        }

        public static void GoToStoryScene()
        {
            ChangeScene("Story", DefaultFadeTime);
        }

        public static void GoToCombatScene()
        {
            ChangeScene("Combat", 5f);
        }

        public static void GoToMainMenuScene()
        {
            ChangeScene("Menu", DefaultFadeTime);
        }

        public static void GoToCreditsScene()
        {
            ChangeScene("Credits", DefaultFadeTime);
        }

        public static void GoToGameScene(Action<float> loadProgressAction = null)
        {
            ChangeScene("Game", DefaultFadeTime, loadProgressAction);
        }

        private static void ChangeScene(string sceneName, float fadeTime, Action<float> loadProgressAction = null)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName, fadeTime, loadProgressAction));
        }
    }
}