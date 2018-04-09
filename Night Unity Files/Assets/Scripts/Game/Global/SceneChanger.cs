using System.Collections;
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
        public static bool Fading;

        public void Awake()
        {
            _instance = this;
            _fader = GameObject.Find("Screen Fader").GetComponent<Image>();
            StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn(float duration = 1f)
        {
            Fading = true;
            float age = 0f;
            while (age < duration)
            {
                age += Time.deltaTime;
                _fader.color = new Color(0, 0, 0, 1 - age / duration);
                yield return null;
            }

            _fader.color = UiAppearanceController.InvisibleColour;
            Fading = false;
            if(SceneManager.GetActiveScene().name == "Game") WorldState.UnPause();
        }

        private IEnumerator FadeOut(string sceneName, float duration)
        {
            Fading = true;
            AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync(sceneName);
            sceneLoaded.allowSceneActivation = false;
            float age = 0f;
            while (age < duration)
            {
                age += Time.deltaTime;
                _fader.color = new Color(0, 0, 0, age / duration);
                yield return null;
            }

            _fader.color = Color.black;
            while (sceneLoaded.progress != 0.9f) yield return null;
            sceneLoaded.allowSceneActivation = true;
        }

        public static void ChangeScene(string sceneName, float duration = 1f)
        {
            WorldState.Pause();
            _instance.StartCoroutine(_instance.FadeOut(sceneName, duration));
        }
    }
}