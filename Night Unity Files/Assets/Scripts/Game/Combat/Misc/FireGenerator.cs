using Fastlights;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FireGenerator : MonoBehaviour
    {
        private static readonly ObjectPool<FireGenerator> _firePool = new ObjectPool<FireGenerator>("Prefabs/Combat/Effects/Fire");
        private static Transform _fireParent;
        private FastLight _light;
        private float _randomSeed;
        private float _startRadius;

        public static void Create(Vector2 position, float radius = 1)
        {
            if (_fireParent == null) _fireParent = GameObject.Find("Fires").transform;
            FireGenerator fire = _firePool.Create(_fireParent);
            fire.transform.position = position;
            fire._light.Radius = radius;
            fire._startRadius = radius;
        }

        public void Awake()
        {
            _light = Helper.FindChildWithName<FastLight>(gameObject, "Light");
            _randomSeed = Random.Range(0f, 5f);
        }

        public void Update()
        {
            Color c = _light.Colour;
            float brightness = Mathf.PerlinNoise(Time.time * 2, _randomSeed) * 0.25f + 0.05f;
            c.a = brightness;
            _light.Radius = _startRadius * (1 - brightness);
            _light.Colour = c;
        }

        private void OnDestroy()
        {
            _firePool.Dispose(this);
        }
    }
}