﻿using System;
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
        private bool Fading;
        public Action _callback;

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