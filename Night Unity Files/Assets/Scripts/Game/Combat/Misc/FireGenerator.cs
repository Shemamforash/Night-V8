using Fastlights;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class FireGenerator : MonoBehaviour
    {
        private static readonly ObjectPool<FireGenerator> _firePool = new ObjectPool<FireGenerator>("Fires", "Prefabs/Combat/Effects/Fire");
        private FastLight _light;
        private float _randomSeed;

        public static void Create(Vector2 position, float radius = 1)
        {
            FireGenerator fire = _firePool.Create();
            fire.transform.position = position;
            fire._light.Radius = radius;
        }

        public void Awake()
        {
            _light = gameObject.FindChildWithName<FastLight>("Light");
            _randomSeed = Random.Range(0f, 5f);
        }

        public void Update()
        {
            Color c = _light.Colour;
            float brightness = Mathf.PerlinNoise(Time.time * 2, _randomSeed) * 0.25f + 0.05f;
            c.a = brightness;
            _light.Colour = c;
        }

        private void OnDestroy()
        {
            _firePool.Dispose(this);
        }
    }
}