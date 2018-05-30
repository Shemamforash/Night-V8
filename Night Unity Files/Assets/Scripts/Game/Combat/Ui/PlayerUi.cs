
using System.Collections;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class PlayerUi : CharacterUi
    {
        private static PlayerUi _instance;
        private float _currentAlpha;
        private Coroutine _fadeInCoroutine, _fadeOutCoroutine;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _currentAlpha = CanvasGroup.alpha;
        }

        public static PlayerUi Instance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<PlayerUi>();
            return _instance;
        }

        public void Hide()
        {
            if(_fadeInCoroutine != null) StopCoroutine(_fadeInCoroutine);
            _fadeOutCoroutine = StartCoroutine(FadeOut());
        }

        public void Show()
        {
            if (_fadeOutCoroutine != null) StopCoroutine(_fadeOutCoroutine);
            _fadeInCoroutine = StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            while (_currentAlpha < 1)
            {
                _currentAlpha += Time.deltaTime;
                if (_currentAlpha > 1) _currentAlpha = 1;
                SetAlpha(_currentAlpha);
                UiAimController.SetAlpha(_currentAlpha);
                yield return null;
            }
        }

        private IEnumerator FadeOut()
        {
            while (_currentAlpha > 0)
            {
                _currentAlpha -= Time.deltaTime;
                if (_currentAlpha < 0) _currentAlpha = 0;
                SetAlpha(_currentAlpha);
                UiAimController.SetAlpha(_currentAlpha);
                yield return null;
            }
        }
    }
}