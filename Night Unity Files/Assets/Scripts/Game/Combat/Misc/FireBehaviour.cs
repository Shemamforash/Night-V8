using System.Collections;
using System.Collections.Generic;
using Fastlights;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FireBehaviour : MonoBehaviour
    {
        private const int MaxEmissionRate = 400;
        private const float LightMaxRadius = 5f;
        private const float LifeTime = 4f;
        private static readonly ObjectPool<FireBehaviour> _firePool = new ObjectPool<FireBehaviour>("Prefabs/Combat/Effects/Fire Area");
        private float _age;
        private FastLight _light;
        private ParticleSystem _particles;
        private CircleCollider2D _collider;
        private int EmissionRate;
        private static Transform _fireParent;
        private bool _keepAlive;

        public void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
            _light = gameObject.FindChildWithName<FastLight>("Light");
            _collider = GetComponent<CircleCollider2D>();
            if (_fireParent == null) _fireParent = GameObject.Find("Fires").transform;
        }

        public static FireBehaviour Create(Vector3 position, float size, bool keepAlive = false, bool lightOn = true)
        {
            FireBehaviour fire = _firePool.Create(_fireParent);
            fire.transform.position = position;
            fire._keepAlive = keepAlive;
            fire.StartCoroutine(fire.Burn(position, size, lightOn));
            return fire;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            CharacterCombat character = other.GetComponent<CharacterCombat>();
            if (character == null) return;
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
    }
}