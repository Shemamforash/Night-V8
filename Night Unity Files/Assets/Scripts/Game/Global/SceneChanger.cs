using System.Collections;
using DG.Tweening;
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

        public void Awake()
        {
            _instance = this;
            _fader = GameObject.Find("Screen Fader").GetComponent<Image>();
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_fader.DOColor(InvisibleBlack, 1f));
            if (SceneManager.GetActiveScene().name != "Game") return;
            sequence.AppendCallback(WorldState.UnPause);
        }

        private IEnumerator FadeOut(string sceneName, bool fade)
        {
            AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync(sceneName);
            sceneLoaded.allowSceneActivation = false;
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