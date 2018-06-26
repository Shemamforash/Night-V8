using System.Collections;
using UnityEngine;

namespace SamsHelper
{
    public class FadeAndDieTrailRenderer : MonoBehaviour
    {
        private float _age;
        private float _opacityA, _opacityB;
        private TrailRenderer _trailRenderer;
        private bool Fading;
        public float LifeTime;

        public void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
            _opacityA = _trailRenderer.startColor.a;
            _opacityB = _trailRenderer.endColor.a;
        }

        public void StartFade(float lifeTime)
        {
            if (Fading) return;
            Fading = true;
            LifeTime = lifeTime;
            _age = LifeTime;
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            while (_age > 0)
            {
                float normalisedLifeTime = _age / LifeTime;
                Color startColour = _trailRenderer.startColor;
                startColour.a = normalisedLifeTime * _opacityA;
                Color endColour = _trailRenderer.endColor;
                endColour.a = normalisedLifeTime * _opacityB;
                _trailRenderer.startColor = startColour;
                _trailRenderer.endColor = endColour;
                _age -= Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}