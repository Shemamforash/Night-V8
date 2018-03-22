using System.Collections;
using System.Collections.Generic;
using LOS;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat
{
    public class Explosion : MonoBehaviour
    {
        private SpriteRenderer _explosionSprite;
        private ParticleSystem _particles;
        private LOSRadialLight _light;
        private float _explodeTime = 0.2f;
        private float _fadeTime = 0.5f;
        private float _explosionWarmupTime = 1f;
        private SpriteRenderer _warningRing;
        private float _age;
        private float _explosionRadius = 1f;
        private float _originalExplosionWidth, _originalWarningWidth;

        private int _damage;

        private float _knockbackDistance;
        private bool _bleed, _burn, _sick, _pierce;

        private static GameObject _explosionPrefab;
        private static readonly List<Explosion> _explosionPool = new List<Explosion>();


        public void Awake()
        {
            _warningRing = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Warning");
            _explosionSprite = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Explosion");
            _particles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Fragments");
            _light = Helper.FindChildWithName<LOSRadialLight>(gameObject, "Light");

            _explosionSprite.color = UiAppearanceController.InvisibleColour;
            _light.color = UiAppearanceController.InvisibleColour;

            _originalExplosionWidth = _explosionSprite.bounds.size.x;
            _originalWarningWidth = _warningRing.bounds.size.x;
        }

        public void OnDestroy()
        {
            _explosionPool.Remove(this);
        }
        
        public static Explosion CreateExplosion(Vector2 position, float radius, int damage)
        {
            Explosion explosion = GetNewExplosion();
            explosion.Initialise(position, radius, damage);
            return explosion;
        }

        private void Initialise(Vector2 position, float radius, int damage)
        {
            transform.position = position;
            _explosionRadius = radius;
            _damage = damage;
            _bleed = false;
            _burn = false;
            _sick = false;
            _pierce = false;
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

        private void DealDamage()
        {
            List<CharacterCombat> charactersInRange = CombatManager.GetCharactersInRange(transform.position, _explosionRadius);
            foreach (CharacterCombat c in charactersInRange)
            {
//                if(_pierce) c.ArmourController.TakeDamage(_damage);
//                else
                c.HealthController().TakeDamage(_damage);
//                c.Knockback(_knockbackDistance);
//                if (_bleed) c.Bleeding.AddStack();
//                if (_burn) c.Burn.AddStack();
//                if (_sick) c.Sick.AddStack();
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

            _warningRing.color = UiAppearanceController.InvisibleColour;
            StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            _age = 0f;
            bool emitted = false;
            bool shownWarning = false;
            bool shownLight = false;
            FireBehaviour.StartBurning(transform.position);
            while (_age < _explodeTime + _fadeTime)
            {
                if (!shownWarning && _age < _explodeTime / 2f)
                {
                    _light.color = UiAppearanceController.FadedColour;
//				_light.radius = 1;
                    ScaleSprite(_explosionSprite, _explosionRadius / 4f, _originalExplosionWidth);
                    _explosionSprite.color = Color.white;
                    shownWarning = true;
                }
                else if (!shownLight && _age > _explodeTime / 2f && _age < _explodeTime)
                {
                    _light.color = UiAppearanceController.InvisibleColour;
                    _explosionSprite.color = UiAppearanceController.InvisibleColour;
                    shownLight = true;
                }
                else if (_age > _explodeTime)
                {
                    if (!emitted)
                    {
                        DealDamage();
                        _particles.Emit(100);
                        emitted = true;
//					_light.radius = 3;
                    }

                    float normalisedTime = (_age - _explodeTime) / _fadeTime;
                    float alpha = 1 - normalisedTime;
                    _light.color = new Color(1, 1, 1, alpha * 0.4f);
                    ScaleSprite(_explosionSprite, _explosionRadius * 0.75f + 0.25f * normalisedTime, _originalExplosionWidth);
                    _explosionSprite.color = new Color(1, 1, 1, alpha);
                }

                _age += Time.deltaTime;
                yield return null;
            }

            _explosionSprite.color = UiAppearanceController.InvisibleColour;
            _light.color = UiAppearanceController.InvisibleColour;
            gameObject.SetActive(false);
            _explosionPool.Add(this);
        }

        public void SetKnockbackDistance(float distance)
        {
            _knockbackDistance = distance;
        }

        public void SetBurning()
        {
            _burn = true;
        }

        public void SetBleeding()
        {
            _bleed = true;
        }

        public void SetSickness()
        {
            _sick = true;
        }

        public void SetPiercing()
        {
            _pierce = true;
        }
    }
}