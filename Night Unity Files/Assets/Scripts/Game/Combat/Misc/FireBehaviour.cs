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
        private ParticleSystem _particles;
        private CircleCollider2D _collider;
        private int EmissionRate;
        private bool _keepAlive;
        private List<ITakeDamageInterface> _ignoreTargets;

        public void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
            _light = gameObject.FindChildWithName<FastLight>("Light");
            _collider = GetComponent<CircleCollider2D>();
        }

        public static FireBehaviour Create(Vector3 position, float size, bool keepAlive = false, bool lightOn = true)
        {
            FireBehaviour fire = _firePool.Create();
            fire.transform.position = position;
            fire._keepAlive = keepAlive;
            fire._ignoreTargets = new List<ITakeDamageInterface>();
            fire.StartCoroutine(fire.Burn(position, size, lightOn));
            return fire;
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

        private IEnumerator Burn(Vector3 position, float size, bool lightOn)
        {
            transform.position = position;
            EmissionRate = (int) (size * size * MaxEmissionRate);
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.rateOverTime = EmissionRate;
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.radius = size;
            _collider.radius = size;
            _light.Radius = size * LightMaxRadius;
            _light.gameObject.SetActive(lightOn);
            gameObject.SetActive(true);
            _age = 0f;
            while (_keepAlive) yield return null;
            while (_age < LifeTime)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                float normalisedTime = 1 - _age / LifeTime;
                emission = _particles.emission;
                emission.rateOverTime = (int) (normalisedTime * EmissionRate);
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