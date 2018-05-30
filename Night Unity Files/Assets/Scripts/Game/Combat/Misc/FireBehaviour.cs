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
        private int EmissionRate;

        public void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
            _light = Helper.FindChildWithName<FastLight>(gameObject, "Light");
        }

        public static void Create(Vector3 position, float size, bool lightOn = true)
        {
            FireBehaviour fire = GetNewFire();
            fire.StartCoroutine(fire.Burn(position, size, lightOn));
        }

        private IEnumerator Burn(Vector3 position, float size, bool lightOn)
        {
            EmissionRate = (int) (size * size * MaxEmissionRate);
            Debug.Log(EmissionRate);
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.radius = size;
            _light.Radius = size * LightMaxRadius;
            _light.gameObject.SetActive(lightOn);
            gameObject.SetActive(true);
            transform.position = position;
            _age = 0f;
            while (_age < LifeTime)
            {
                float normalisedTime = 1 - _age / LifeTime;
                ParticleSystem.EmissionModule emission = _particles.emission;
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
        }

        private static FireBehaviour GetNewFire()
        {
            if (_firePool.Count == 0)
            {
                if (_firePrefab == null) _firePrefab = Resources.Load<GameObject>("Prefabs/Combat/Fire Area");
                GameObject fireObject = Instantiate(_firePrefab);
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