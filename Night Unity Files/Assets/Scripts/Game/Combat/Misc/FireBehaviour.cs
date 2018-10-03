using System.Collections;
using System.Collections.Generic;
using Fastlights;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FireBehaviour : MonoBehaviour
    {
        private const int MaxEmissionRate = 100;
        private const float LightMaxRadius = 5f;
        private const float LifeTime = 8f;
        private static readonly ObjectPool<FireBehaviour> _firePool = new ObjectPool<FireBehaviour>("Fire Areas", "Prefabs/Combat/Effects/Fire Area");
        private float _age;
        private FastLight _light;
        private ParticleSystem _flames, _embers;
        private CircleCollider2D _collider;
        private int EmissionRate;
        private bool _keepAlive;
        private List<ITakeDamageInterface> _ignoreTargets;
        private float _lifeTime;
        private SpriteRenderer _flash;

        public void Awake()
        {
            _flames = GetComponent<ParticleSystem>();
            _embers = gameObject.FindChildWithName<ParticleSystem>("Embers");
            _flash = gameObject.FindChildWithName<SpriteRenderer>("Flash");
            _light = gameObject.FindChildWithName<FastLight>("Light");
            _collider = GetComponent<CircleCollider2D>();
        }

        public static FireBehaviour Create(Vector3 position, float size, float lifeTime = LifeTime, bool keepAlive = false, bool lightOn = true)
        {
            FireBehaviour fire = _firePool.Create();
            fire.Initialise(position, size, lifeTime, keepAlive, lightOn);
            return fire;
        }

        private void Initialise(Vector3 position, float size, float lifeTime = LifeTime, bool keepAlive = false, bool lightOn = true)
        {
            _lifeTime = lifeTime;
            _keepAlive = keepAlive;
            _ignoreTargets = new List<ITakeDamageInterface>();
            StartCoroutine(Burn(position, size, lightOn));
        }

        public void AddIgnoreTarget(ITakeDamageInterface _ignoreTarget)
        {
            _ignoreTargets.Add(_ignoreTarget);
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if (!CombatManager.IsCombatActive()) return;
            CharacterCombat character = other.GetComponent<CharacterCombat>();
            if (character == null) return;
            if (_ignoreTargets.Contains(other.GetComponent<ITakeDamageInterface>())) return;
            character.Burn();
        }

        public void LetDie()
        {
            _keepAlive = false;
        }

        private void SetFireSize(float emissionRate, float size)
        {
            ParticleSystem.EmissionModule emission = _flames.emission;
            emission.rateOverTime = emissionRate;
            ParticleSystem.ShapeModule shape = _flames.shape;
            shape.radius = size;
            emission = _embers.emission;
            emission.rateOverTime = emissionRate * 0.1f;
            shape = _embers.shape;
            shape.radius = size;
        }

        private void ResetFire(Vector3 position, float size, bool lightOn)
        {
            transform.position = position;
            EmissionRate = (int) (size * size * MaxEmissionRate);
            SetFireSize(EmissionRate, size);
            _collider.radius = size;
            _light.Radius = size * LightMaxRadius;
            _light.gameObject.SetActive(lightOn);
            gameObject.SetActive(true);
            StartCoroutine(Flash());
            _flash.transform.localScale = Vector2.one * size;
        }

        private IEnumerator Flash()
        {
            float flashTime = 0.5f;
            Color c = _flash.color;
            c.a = 1;
            _flash.color = c;
            while (flashTime > 0f)
            {
                flashTime -= Time.deltaTime;
                float normalisedTime = flashTime / 0.5f;
                c.a = normalisedTime;
                _flash.color = c;
                yield return null;
            }

            c.a = 0;
            _flash.color = c;
        }

        private IEnumerator Burn(Vector3 position, float size, bool lightOn)
        {
            ResetFire(position, size, lightOn);
            _age = 0f;
            while (_keepAlive) yield return null;
            while (_age < _lifeTime)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                float normalisedTime = 1 - _age / LifeTime;
                float newEmissionRate = normalisedTime * EmissionRate;
                SetFireSize(newEmissionRate, size);
                _light.Colour = new Color(0.6f, 0.1f, 0.1f, 0.6f * normalisedTime * size);
                _age += Time.deltaTime;
                yield return null;
            }

            _firePool.Return(this);
        }

        private void OnDestroy()
        {
            _firePool.Dispose(this);
        }

        public void AddIgnoreTargets(List<ITakeDamageInterface> targetsToIgnore)
        {
            _ignoreTargets.AddRange(targetsToIgnore);
        }
    }
}