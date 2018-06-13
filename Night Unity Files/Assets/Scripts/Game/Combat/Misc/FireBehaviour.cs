using System.Collections;
using System.Collections.Generic;
using Fastlights;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FireBehaviour : MonoBehaviour
    {
        private const int MaxEmissionRate = 500;
        private const float LightMaxRadius = 5f;
        private const float LifeTime = 4f;
        private static readonly List<FireBehaviour> _firePool = new List<FireBehaviour>();
        private static GameObject _firePrefab;
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
            _light = Helper.FindChildWithName<FastLight>(gameObject, "Light");
            _collider = GetComponent<CircleCollider2D>();
        }

        public static FireBehaviour Create(Vector3 position, float size, bool keepAlive = false, bool lightOn = true)
        {
            FireBehaviour fire = GetNewFire();
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

            gameObject.SetActive(false);
            _firePool.Add(this);
        }

        private void OnDestroy()
        {
            _firePool.Remove(this);
            if (_firePool.Count == 0) _fireParent = null;
        }

        private static FireBehaviour GetNewFire()
        {
            if (_firePool.Count == 0)
            {
                if (_firePrefab == null) _firePrefab = Resources.Load<GameObject>("Prefabs/Combat/Fire Area");
                GameObject fireObject = Instantiate(_firePrefab);
                if (_fireParent == null) _fireParent = GameObject.Find("Fires").transform;
                fireObject.transform.SetParent(_fireParent, false);
                fireObject.transform.localScale = Vector3.one;
                return fireObject.GetComponent<FireBehaviour>();
            }

            FireBehaviour fire = _firePool[0];
            _firePool.RemoveAt(0);
            fire.gameObject.SetActive(true);
            return fire;
        }
    }
}