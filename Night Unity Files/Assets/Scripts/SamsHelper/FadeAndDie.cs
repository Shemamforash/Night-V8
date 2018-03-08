using System.Collections;
using UnityEngine;

namespace SamsHelper
{
    public class FadeAndDie : MonoBehaviour
    {
        private float _fullOpacity;
        private float _age;
        public float LifeTime;
        private SpriteRenderer _spriteRenderer;
        public bool FadeOnAwake;
        private bool Fading;

        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _fullOpacity = _spriteRenderer.color.a;
            if (FadeOnAwake)
            {
                StartFade(LifeTime);
            }
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
                _spriteRenderer.color = new Color(1f, 1f, 1f, normalisedLifeTime * _fullOpacity);
                _age -= Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}