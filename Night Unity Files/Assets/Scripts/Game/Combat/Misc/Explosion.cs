using System;
using System.Collections;
using System.Collections.Generic;
using Fastlights;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class Explosion : MonoBehaviour
    {
        private static readonly ObjectPool<Explosion> _explosionPool = new ObjectPool<Explosion>("Explosions", "Prefabs/Combat/Visuals/Explosion");

        private const float ExplodeTime = 0.2f;
        private const float ExplosionWarmupTime = 1f;
        private const float FadeTime = 0.5f;

        private float _age;
        private int _damage;
        private float _explosionRadius = 1f;
        private SpriteRenderer _explosionSprite;

        private FastLight _light;
        private ParticleSystem _fragments, _smoke;
        private SpriteRenderer _warningRing;
        private AudioSource _audioSource;
        private Action<List<EnemyBehaviour>> OnExplode;
        private GameObject _spriteObject;

        private static AudioClip[] _explosionClips;
        private bool _decay, _incendiary, _sicken;
        private List<CanTakeDamage> _targetsToIgnore;

        public void Awake()
        {
            _warningRing = gameObject.FindChildWithName<SpriteRenderer>("Warning");
            _explosionSprite = gameObject.FindChildWithName<SpriteRenderer>("Explosion");
            _fragments = gameObject.FindChildWithName<ParticleSystem>("Fragments");
            _smoke = gameObject.FindChildWithName<ParticleSystem>("Smoke");
            _light = gameObject.FindChildWithName<FastLight>("Light");
            _spriteObject = gameObject.FindChildWithName("Sprites");
            _audioSource = gameObject.FindChildWithName<AudioSource>("Audio");

            _explosionSprite.color = UiAppearanceController.InvisibleColour;
            _light.Colour = UiAppearanceController.InvisibleColour;

            if (_explosionClips == null) _explosionClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("combat/explosions");
        }

        public void OnDestroy()
        {
            _explosionPool.Dispose(this);
        }

        public static Explosion CreateExplosion(Vector2 position, int damage, float radius = 1)
        {
            Explosion explosion = _explosionPool.Create();
            explosion.Initialise(position, damage, radius);
            return explosion;
        }

        public void SetBurn()
        {
            _incendiary = true;
        }

        public void SetDecay()
        {
            _decay = true;
        }

        public void AddIgnoreTarget(CanTakeDamage ignoreTarget)
        {
            _targetsToIgnore.Add(ignoreTarget);
        }

        public void AddIgnoreTargets(List<CanTakeDamage> ignoreTargets)
        {
            _targetsToIgnore.AddRange(ignoreTargets);
        }

        private void Initialise(Vector2 position, int damage, float radius = 1)
        {
            transform.position = position;
            _spriteObject.transform.localScale = Vector2.one * radius;
            _explosionRadius = radius;
            _damage = damage;
            _incendiary = false;
            _decay = false;
            _targetsToIgnore = new List<CanTakeDamage>();
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
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
            List<EnemyBehaviour> enemiesHit = new List<EnemyBehaviour>();
            foreach (Collider2D col in colliders)
            {
                CanTakeDamage i = col.GetComponent<CanTakeDamage>();
                if (i == null) continue;
                if (_targetsToIgnore.Contains(i)) return;
                i.TakeExplosionDamage(_damage, transform.position, _explosionRadius);
                EnemyBehaviour behaviour = i as EnemyBehaviour;
                if (behaviour != null) enemiesHit.Add(behaviour);
            }

            OnExplode?.Invoke(enemiesHit);
        }

        private IEnumerator Warmup()
        {
            _age = 0f;
            while (_age < ExplosionWarmupTime)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                _warningRing.transform.Rotate(0, 0, 5 * Time.deltaTime);
                _warningRing.color = new Color(1, 0, 0, _age / ExplosionWarmupTime);
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
            while (_age < ExplodeTime + FadeTime)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
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
                        _audioSource.clip = _explosionClips.RandomElement();
                        _audioSource.Play();
                        AddConditions();
                        DealDamage();
                        EmitParticles();
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
            while (_audioSource.isPlaying) yield return null;
            _explosionPool.Return(this);
        }

        private void EmitParticles()
        {
            ParticleSystem.ShapeModule shape = _fragments.shape;
            shape.radius = _explosionRadius;
            int fragments = (int) ((_explosionRadius - 0.5f) * 85f + 100);
            _fragments.Emit(fragments);
            ParticleSystem.MainModule main = _smoke.main;
            main.startSpeed = _explosionRadius;
            _smoke.Emit((int) (_explosionRadius * 30f));
        }

        private void AddConditions()
        {
            if (_incendiary) FireBehaviour.Create(transform.position, 1).AddIgnoreTargets(_targetsToIgnore);
            if (_decay) DecayBehaviour.Create(transform.position).AddIgnoreTargets(_targetsToIgnore);
            if (_sicken) SickenBehaviour.Create(transform.position, _targetsToIgnore);
        }

        public void AddOnDetonate(Action<List<EnemyBehaviour>> action)
        {
            OnExplode += action;
        }

        public void SetSicken()
        {
            _sicken = true;
        }
    }
}