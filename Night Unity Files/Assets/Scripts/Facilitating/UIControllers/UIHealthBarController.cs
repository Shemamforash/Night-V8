using System;
using System.Collections;
using System.Collections.Generic;
using SamsHelper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UIHealthBarController : MonoBehaviour
    {
        private Slider _slider;
        private RectTransform _fill;
        private ParticleSystem _bleedEffect, _burnEffect;
        private RectTransform _sliderRect;
        private float _edgeWidthRatio = 3f;
        private TextMeshProUGUI _healthText;
        private static readonly List<Fader> _faderPool = new List<Fader>();
        
        public void Awake()
        {
            GameObject healthBar = Helper.FindChildWithName(gameObject, "Health Bar");
            _fill = Helper.FindChildWithName<RectTransform>(healthBar, "Fill");
            _slider = healthBar.GetComponent<Slider>();
            _burnEffect = Helper.FindChildWithName<ParticleSystem>(healthBar, "Burning");
            _bleedEffect = Helper.FindChildWithName<ParticleSystem>(healthBar, "Bleeding");
            _healthText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Health Text");
        }

        public void SetIsPlayerBar()
        {
            _edgeWidthRatio = 7.2f;
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

        private class Fader : MonoBehaviour
        {
            private Image _faderImage;
            private const float Duration = 0.5f;

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
                StartCoroutine(Fade());
            }

            private IEnumerator Fade()
            {
                float age = 0f;
                while (age < Duration)
                {
                    float alpha = 1 - age / Duration;
                    _faderImage.color = new Color(1, 0, 0, alpha);
                    age += Time.deltaTime;
                    yield return null;
                }
                gameObject.SetActive(false);
                _faderPool.Add(this);
            }
        }

        public void SetValue(float normalised, float current, float max)
        {
            float normalisedHealth = normalised;
            _healthText.text = (int) current + "/" + (int) max;
            _slider.value = normalisedHealth;
            float edgeWidth = _edgeWidthRatio * normalisedHealth;

            ParticleSystem.ShapeModule burnShape = _burnEffect.shape;
            burnShape.radius = edgeWidth;

            ParticleSystem.EmissionModule burnEmission = _burnEffect.emission;
            burnEmission.rateOverTime = (int) (edgeWidth / _edgeWidthRatio * 50f);

            ParticleSystem.ShapeModule bleedShapeModule = _bleedEffect.shape;
            bleedShapeModule.radius = edgeWidth;

            ParticleSystem.EmissionModule bleedEmission = _bleedEffect.emission;
            bleedEmission.rateOverTime = (int) (edgeWidth / _edgeWidthRatio * 10f);
        }

        public void StartBleeding()
        {
            if (_bleedEffect.isPlaying) return;
            _bleedEffect.Play();
        }

        public void StopBleeding()
        {
            _bleedEffect.Stop();
        }

        public void StartBurning()
        {
            if (_burnEffect.isPlaying) return;
            _burnEffect.Play();
        }

        public void StopBurning()
        {
            _burnEffect.Stop();
        }
    }
}