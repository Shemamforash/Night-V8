using System;
using Game.Global;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Facilitating
{
    public class Campfire : MonoBehaviour
    {
        private static float _intensity;
        private static bool _tending;
        private static float _intensityLoss;
        private Image _fireImage;
        private float _x;
        public float ClampMin, ClampMax = 1;
        public int EmissionRate = 150;
        public ParticleSystem FireSystem;
        public float FlickerRate = 0.1f;
        public int TimeToDie = 3;
        private ParticleSystem.EmissionModule emission;
        public bool Eternal;

        public void Start()
        {
            _fireImage = GetComponent<Image>();
            _intensityLoss = 1f / (TimeToDie * WorldState.MinutesPerHour);
            emission = FireSystem.emission;
            emission.rateOverTime = EmissionRate;
            if (Eternal) _intensity = 1;
        }

        public static float Intensity()
        {
            return _intensity;
        }

        public static void Die()
        {
            _intensity -= _intensityLoss;
            if (_intensity < 0) _intensity = 0;
        }

        public static void Tend()
        {
            _intensity += 0.1f;
            if (_intensity > 1) _intensity = 1;
        }

        public static bool IsLit()
        {
            return _intensity > 0;
        }

        public void Update()
        {
            if(!Eternal) emission.rateOverTime = EmissionRate * _intensity;
            _x += FlickerRate;
            if (_x > 1) _x -= 1;
            float newOpacity = Mathf.PerlinNoise(_x, 0);
            if (ClampMin >= ClampMax) throw new Exception("Clamp min should not be greater than or equal to clamp max.");
            float clampDiff = ClampMax - ClampMin;
            newOpacity = newOpacity * clampDiff + ClampMin;
            Color oldColor = _fireImage.color;
            oldColor.a = newOpacity * _intensity;
            _fireImage.color = oldColor;
        }
    }
}