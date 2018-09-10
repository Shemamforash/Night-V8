using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating
{
    public class Campfire : MonoBehaviour
    {
        private ParticleSystem _fire, _smoke1, _smoke2, _smoke3, _smoke4;
        private const float FireEmissionRateMax = 20;
        private const float Smoke1DurationMax = 20;
        private const float Smoke2DurationMax = 10;
        private const float Smoke3DurationMax = 5;
        private const float Smoke4DurationMax = 3;
        private static float _fireLevel;
        private const float FireBurnOutPoint = 0.2f;
        private Image _fireLightImage;
        
        public void Start()
        {
            _fire = gameObject.FindChildWithName<ParticleSystem>("Fire");
            _smoke1 = gameObject.FindChildWithName<ParticleSystem>("Smoke 1");
            _smoke2 = gameObject.FindChildWithName<ParticleSystem>("Smoke 2");
            _smoke3 = gameObject.FindChildWithName<ParticleSystem>("Smoke 3");
            _smoke4 = gameObject.FindChildWithName<ParticleSystem>("Smoke 4");
            _fireLightImage = GetComponent<Image>();
            Restart();
        }

        private void Restart()
        {
            UpdateRates();
            _fire.Clear();
            _smoke1.Clear();
            _smoke2.Clear();
            _smoke3.Clear();
            _smoke4.Clear();
        }

        private void UpdateRates()
        {
            ParticleSystem.EmissionModule fireEmission = _fire.emission;
            float fireEmissionLevel = _fireLevel - FireBurnOutPoint;
            if (fireEmissionLevel < 0) fireEmissionLevel = 0;
            fireEmissionLevel /= (1 - FireBurnOutPoint);
            fireEmission.rateOverTime = FireEmissionRateMax * fireEmissionLevel;

            ParticleSystem.MainModule _smoke1Main = _smoke1.main;
            _smoke1Main.startLifetime = Smoke1DurationMax * _fireLevel;
            ParticleSystem.MainModule _smoke2Main = _smoke2.main;
            _smoke2Main.startLifetime = Smoke2DurationMax * _fireLevel;
            ParticleSystem.MainModule _smoke3Main = _smoke3.main;
            _smoke3Main.startLifetime = Smoke3DurationMax * _fireLevel;
            ParticleSystem.MainModule _smoke4Main = _smoke4.main;
            _smoke4Main.startLifetime = Smoke4DurationMax * _fireLevel;

            _fireLightImage.color = Color.Lerp(Color.black, Color.white, _fireLevel);
        }
        
        public void Update()
        {
            UpdateRates();
        }
        
        public static void Tend()
        {
            Debug.Log(_fireLevel);
            _fireLevel += 0.2f;
            if (_fireLevel > 1) _fireLevel = 1;
        }

        public static void Die()
        {
            _fireLevel -= 0.01f;
            if (_fireLevel < 0) _fireLevel = 0;
        }

        public static bool IsLit()
        {
            return _fireLevel > FireBurnOutPoint;
        }
    }
}