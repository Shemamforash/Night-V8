﻿using System.Collections;
using DG.Tweening;
using Facilitating.Audio;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Global
{
    public class SceneChanger : MonoBehaviour
    {
        private static Image _fader;
        private static SceneChanger _instance;
        private static readonly Color InvisibleBlack = new Color(0, 0, 0, 0);
        private AudioSource _audioSource;

        public void Awake()
        {
            _instance = this;
            _audioSource = GetComponent<AudioSource>();
            _fader = GameObject.Find("Screen Fader").GetComponent<Image>();
            GlobalAudioManager.SetVolume(0f);
            DOTween.To(GlobalAudioManager.Volume, GlobalAudioManager.SetVolume, 1f, 1f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_fader.DOColor(InvisibleBlack, 1f));
            if (SceneManager.GetActiveScene().name != "Game") return;
            sequence.AppendCallback(WorldState.UnPause);
        }

        private IEnumerator FadeOut(string sceneName, bool fade)
        {
            AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync(sceneName);
            sceneLoaded.allowSceneActivation = false;
            if (_audioSource != null) _audioSource.DOFade(1, 1);
            GlobalAudioManager.SetVolume(1f);
            DOTween.To(GlobalAudioManager.Volume, GlobalAudioManager.SetVolume, 0f, 1f);
            if (fade)
            {
                yield return _fader.DOColor(Color.black, 1f).WaitForCompletion();
            }

            while (sceneLoaded.progress != 0.9f) yield return null;
            sceneLoaded.allowSceneActivation = true;
        }

        public static void ChangeScene(string sceneName, bool fade = true)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName, fade));
        }
    }
}