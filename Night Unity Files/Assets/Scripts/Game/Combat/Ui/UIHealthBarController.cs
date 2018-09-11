using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Ui
{
    public class UIHealthBarController : MonoBehaviour
    {
        private static readonly List<Fader> _faderPool = new List<Fader>();
        private ParticleSystem _burnEffect;
        private RectTransform _fill;
        private TextMeshProUGUI _healthText;
        private Slider _slider;
        private RectTransform _sliderRect;
        private Image _sicknessImage;
        private bool _cached;

        public void Awake()
        {
            GameObject healthBar = gameObject.FindChildWithName("Health Bar");
            _fill = healthBar.FindChildWithName<RectTransform>("Fill");
            _slider = healthBar.GetComponent<Slider>();
            _burnEffect = healthBar.FindChildWithName<ParticleSystem>("Burning");
            _healthText = gameObject.FindChildWithName<TextMeshProUGUI>("Health Text");
            _sicknessImage = gameObject.FindChildWithName<Image>("Sickness");
        }

        public void FadeNewHealth()
        {
            Fader fader;
            GameObject faderObject;
            if (_faderPool.Count == 0)
            {
                faderObject = new GameObject();
                faderObject.name = "Fader";
                faderObject.transform.SetParent(_fill.parent, false);
                faderObject.AddComponent<Image>();
                fader = faderObject.AddComponent<Fader>();
            }
            else
            {
                fader = _faderPool[0];
                _faderPool.RemoveAt(0);
                faderObject = fader.gameObject;
                faderObject.SetActive(true);
            }

            faderObject.transform.SetSiblingIndex(1);
            RectTransform faderTransform = fader.GetComponent<RectTransform>();
            faderTransform.anchorMin = Vector2.zero;
            faderTransform.anchorMax = new Vector2(_fill.anchorMax.x, 1);
            faderTransform.offsetMin = Vector2.zero;
            faderTransform.offsetMax = Vector2.zero;
            fader.Restart();
        }

        public void SetValue(Number health)
        {
            _healthText.text = (int) health.CurrentValue() + "/" + (int) health.Max;
            _slider.value = health.Normalised();
        }

        public void StartBurning()
        {
            if (_burnEffect.isPlaying) return;
            _burnEffect.Play();
        }

        public void SetSicknessLevel(float normalisedValue)
        {
            _sicknessImage.DOFillAmount(normalisedValue, 0.1f);
        }

        public void StopBurning()
        {
            _burnEffect.Stop();
        }

        private class Fader : MonoBehaviour
        {
            private const float Duration = 0.5f;
            private Image _faderImage;

            public void Awake()
            {
                _faderImage = GetComponent<Image>();
            }

            private void OnDestroy()
            {
                _faderPool.Remove(this);
            }

            public void Restart()
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(_faderImage.DOColor(new Color(0, 0, 0, 0), Duration));
                seq.AppendCallback(() =>
                {
                    gameObject.SetActive(false);
                    _faderPool.Add(this);
                });
            }
        }
    }
}