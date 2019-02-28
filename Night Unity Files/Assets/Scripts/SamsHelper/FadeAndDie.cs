using System;
using System.Collections;
using Game.Combat.Generation;
using UnityEngine;

namespace SamsHelper
{
    public class FadeAndDie : MonoBehaviour
    {
        private float _age;
        public Action _callback;
        private float _fullOpacity;
        private SpriteRenderer _spriteRenderer;
        private bool Fading;
        public float LifeTime;

        public void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _fullOpacity = _spriteRenderer.color.a;
        }

        public void SetLifeTime(float lifeTime, Action callback = null)
        {
            _callback = callback;
            LifeTime = lifeTime;
        }

        public void StartFade()
        {
            if (Fading) return;
            _spriteRenderer.color = new Color(1f, 1f, 1f, _fullOpacity);
            Fading = true;
            _age = LifeTime;
            StartCoroutine(Fade());
        }

        private IEnumerator Fade()
        {
            while (_age > 0)
            {
                if (!CombatManager.Instance().IsCombatActive()) yield return null;
                float normalisedLifeTime = _age / LifeTime;
                _spriteRenderer.color = new Color(1f, 1f, 1f, normalisedLifeTime * _fullOpacity);
                _age -= Time.deltaTime;
                yield return null;
            }

            if (_callback == null)
            {
                Destroy(gameObject);
            }
            else
            {
                _callback();
                Fading = false;
            }
        }
    }
}