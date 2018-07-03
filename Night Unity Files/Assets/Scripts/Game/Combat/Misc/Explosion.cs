using System.Collections;
using System.Collections.Generic;
using Fastlights;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class Explosion : MonoBehaviour
    {
        private static GameObject _explosionPrefab;
        private static readonly List<Explosion> _explosionPool = new List<Explosion>();
        private float _age;

        private int _damage;
        private readonly float _explodeTime = 0.2f;
        private float _explosionRadius = 1f;
        private SpriteRenderer _explosionSprite;
        private readonly float _explosionWarmupTime = 1f;
        private readonly float _fadeTime = 0.5f;

        private FastLight _light;
        private float _originalExplosionWidth, _originalWarningWidth;
        private ParticleSystem _particles;
        private SpriteRenderer _warningRing;


        public void Awake()
        {
            _warningRing = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Warning");
            _explosionSprite = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Explosion");
            _particles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Fragments");
            _light = Helper.FindChildWithName<FastLight>(gameObject, "Light");

            _explosionSprite.color = UiAppearanceController.InvisibleColour;
            _light.Colour = UiAppearanceController.InvisibleColour;

            _originalExplosionWidth = _explosionSprite.bounds.size.x;
            _originalWarningWidth = _warningRing.bounds.size.x;
        }

        public void OnDestroy()
        {
            _explosionPool.Remove(this);
        }

        public static Explosion CreateExplosion(Vector2 position, int damage, float radius = 1)
        {
            Explosion explosion = GetNewExplosion();
            explosion.Initialise(position, damage, radius);
            return explosion;
        }

        private void Initialise(Vector2 position, int damage, float radius = 1)
        {
            transform.position = position;
            _explosionRadius = radius;
            _damage = damage;
        }

        private static Explosion GetNewExplosion()
        {
            if (_explosionPool.Count == 0)
            {
                if (_explosionPrefab == null) _explosionPrefab = Resources.Load<GameObject>("Prefabs/Combat/Explosion");
                GameObject explosionObject = Instantiate(_explosionPrefab);
                explosionObject.transform.localScale = Vector3.one;
                return explosionObject.GetComponent<Explosion>();
            }

            Explosion explosion = _explosionPool[0];
            _explosionPool.RemoveAt(0);
            return explosion;
        }

        public void Detonate()
        {
            gameObject.SetActive(true);
            StartCoroutine(Warmup());
        }

        public void InstantDetonate()
        {
            gameObject.SetActive(true);
            StartCoroutine(Explode());
        }

        private void DealDamage()
        {
            List<CharacterCombat> charactersInRange = CombatManager.GetCharactersInRange(transform.position, _explosionRadius);
            foreach (CharacterCombat c in charactersInRange)
            {
                c.HealthController.TakeDamage(_damage);
                Vector2 dir = c.transform.position - transform.position;
                float distance = dir.magnitude;
                dir.Normalize();
                c.AddForce(dir * 1f / distance * 10f);
            }
        }

        private void ScaleSprite(SpriteRenderer sprite, float scalingValue, float originalSize)
        {
            float scaledWidth = scalingValue / originalSize;
            sprite.transform.localScale = Vector3.one * scaledWidth;
        }

        private IEnumerator Warmup()
        {
            _age = 0f;
            ScaleSprite(_warningRing, _explosionRadius, _originalWarningWidth);
            while (_age < _explosionWarmupTime)
            {
                _warningRing.transform.Rotate(0, 0, 5 * Time.deltaTime);
                _warningRing.color = new Color(1, 0, 0, _age / _explosionWarmupTime);
                _age += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            _age = 0f;
            bool emitted = false;
            bool shownWarning = false;
            bool shownLight = false;
            _warningRing.color = UiAppearanceController.InvisibleColour;
            while (_age < _explodeTime + _fadeTime)
            {
                if (!shownWarning && _age < _explodeTime / 2f)
                {
                    _light.Colour = UiAppearanceController.FadedColour;
                    _light.Radius = 1;
                    ScaleSprite(_explosionSprite, _explosionRadius / 8f, _originalExplosionWidth);
                    _explosionSprite.color = Color.white;
                    shownWarning = true;
                }
                else if (!shownLight && _age > _explodeTime / 2f && _age < _explodeTime)
                {
                    _light.Colour = UiAppearanceController.InvisibleColour;
                    _explosionSprite.color = UiAppearanceController.InvisibleColour;
                    shownLight = true;
                }
                else if (_age > _explodeTime)
                {
                    if (!emitted)
                    {
                        DealDamage();
                        ParticleSystem.ShapeModule shape = _particles.shape;
                        shape.radius = _explosionRadius * 0.1f;
                        _particles.Emit(100);
                        emitted = true;
                        _light.Radius = _explosionRadius * 1.5f;
                    }

                    float normalisedTime = (_age - _explodeTime) / _fadeTime;
                    float alpha = 1 - normalisedTime;
                    _light.Colour = new Color(1, 1, 1, alpha * 0.4f);
                    ScaleSprite(_explosionSprite, _explosionRadius * 0.2f + 0.25f * normalisedTime, _originalExplosionWidth);
                    _explosionSprite.color = new Color(1, 1, 1, alpha);
                }

                _age += Time.deltaTime;
                yield return null;
            }

            _explosionSprite.color = UiAppearanceController.InvisibleColour;
            _light.Colour = UiAppearanceController.InvisibleColour;
            gameObject.SetActive(false);
            _explosionPool.Add(this);
        }
    }
}