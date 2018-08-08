using System;
using System.Collections;
using System.Collections.Generic;
using Fastlights;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Misc;
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

        private const float ExplodeTime = 0.2f;
        private const float ExplosionWarmupTime = 1f;
        private const float FadeTime = 0.5f;

        private float _age;
        private int _damage;
        private float _explosionRadius = 1f;
        private SpriteRenderer _explosionSprite;

        private FastLight _light;
        private ParticleSystem _particles;
        private SpriteRenderer _warningRing;
        private Action<List<EnemyBehaviour>> OnExplode;
        private GameObject _spriteObject;

        [SerializeField] private AudioClip[] _explosionClips;


        public void Awake()
        {
            _warningRing = gameObject.FindChildWithName<SpriteRenderer>("Warning");
            _explosionSprite = gameObject.FindChildWithName<SpriteRenderer>("Explosion");
            _particles = gameObject.FindChildWithName<ParticleSystem>("Fragments");
            _light = gameObject.FindChildWithName<FastLight>("Light");
            _spriteObject = gameObject.FindChildWithName("Sprites");

            _explosionSprite.color = UiAppearanceController.InvisibleColour;
            _light.Colour = UiAppearanceController.InvisibleColour;
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
            _spriteObject.transform.localScale = Vector2.one * radius;
            _explosionRadius = radius;
            _damage = damage;
        }

        private static Explosion GetNewExplosion()
        {
            if (_explosionPool.Count == 0)
            {
                if (_explosionPrefab == null) _explosionPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Explosion");
                GameObject explosionObject = Instantiate(_explosionPrefab);
                explosionObject.transform.localScale = Vector3.one;
                return explosionObject.GetComponent<Explosion>();
            }

            Explosion explosion = _explosionPool[0];
            _explosionPool.RemoveAt(0);
            return explosion;
        }

        public void Detonate(Grenade grenade = null)
        {
            gameObject.SetActive(true);
            StartCoroutine(Warmup(grenade));
        }

        public void InstantDetonate()
        {
            gameObject.SetActive(true);
            StartCoroutine(Explode());
        }

        private void DealDamage()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
            List<EnemyBehaviour> enemiesHit = new List<EnemyBehaviour>();
            foreach (Collider2D col in colliders)
            {
                ITakeDamageInterface i = col.GetComponent<ITakeDamageInterface>();
                if (i == null) continue;
                i.TakeExplosionDamage(_damage, transform.position);
                EnemyBehaviour behaviour = i as EnemyBehaviour;
                if (behaviour != null) enemiesHit.Add(behaviour);
            }
            OnExplode?.Invoke(enemiesHit);
        }

        private IEnumerator Warmup(Grenade grenade)
        {
            _age = 0f;
            while (_age < ExplosionWarmupTime)
            {
                _warningRing.transform.Rotate(0, 0, 5 * Time.deltaTime);
                _warningRing.color = new Color(1, 0, 0, _age / ExplosionWarmupTime);
                _age += Time.deltaTime;
                yield return null;
            }

            if(grenade != null) grenade.Deactivate();
            StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            _age = 0f;
            bool emitted = false;
            bool shownWarning = false;
            bool shownLight = false;
            _warningRing.color = UiAppearanceController.InvisibleColour;
            while (_age < ExplodeTime + FadeTime)
            {
                if (!shownWarning && _age < ExplodeTime / 2f)
                {
                    _light.Colour = UiAppearanceController.FadedColour;
                    _light.Radius = 1;
                    _spriteObject.transform.localScale = Vector2.one * _explosionRadius / 8f;
                    _explosionSprite.color = Color.white;
                    shownWarning = true;
                }
                else if (!shownLight && _age > ExplodeTime / 2f && _age < ExplodeTime)
                {
                    _light.Colour = UiAppearanceController.InvisibleColour;
                    _explosionSprite.color = UiAppearanceController.InvisibleColour;
                    shownLight = true;
                }
                else if (_age > ExplodeTime)
                {
                    if (!emitted)
                    {
                        AudioSource.PlayClipAtPoint(_explosionClips.RandomElement(), transform.position);
                        DealDamage();
                        ParticleSystem.ShapeModule shape = _particles.shape;
                        shape.radius = _explosionRadius;
                        _particles.Emit((int) (_explosionRadius * _explosionRadius * 40f));
                        emitted = true;
                        _light.Radius = _explosionRadius * 1.5f;
                    }

                    float normalisedTime = (_age - ExplodeTime) / FadeTime;
                    float alpha = 1 - normalisedTime;
                    _spriteObject.transform.localScale = Vector2.one * (_explosionRadius * 0.75f + 0.25f * normalisedTime);
                    _light.Colour = new Color(1, 1, 1, alpha * 0.4f);
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

        public void AddOnDetonate(Action<List<EnemyBehaviour>> action)
        {
            OnExplode += action;
        }
    }
}