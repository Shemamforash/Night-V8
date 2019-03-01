using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
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
        private const float FireBurnOutPoint = 0.2f;
        private Image _fireLightImage;
        private static bool _tending;
        private static float _fireLevel;
        private static Crackling _crackling;
        private static Campfire _instance;

        public void Awake()
        {
            _crackling = GetComponent<Crackling>();
            _crackling.Silence();
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        public void Start()
        {
            _fire = gameObject.FindChildWithName<ParticleSystem>("Fire");
            _smoke1 = gameObject.FindChildWithName<ParticleSystem>("Smoke 1");
            _smoke2 = gameObject.FindChildWithName<ParticleSystem>("Smoke 2");
            _smoke3 = gameObject.FindChildWithName<ParticleSystem>("Smoke 3");
            _smoke4 = gameObject.FindChildWithName<ParticleSystem>("Smoke 4");
            _fireLightImage = GetComponent<Image>();
            UpdateRates();
        }

        private void SetEmission(ParticleSystem p, int emissionRate, float duration = -1)
        {
            ParticleSystem.EmissionModule emission = p.emission;
            emission.rateOverTime = emissionRate;
            if (duration == -1) return;
            ParticleSystem.MainModule main = p.main;
            main.startLifetime = duration;
        }

        private void UpdateRates()
        {
            float fireEmissionLevel = _fireLevel - FireBurnOutPoint;
            fireEmissionLevel /= 1 - FireBurnOutPoint;
            if (fireEmissionLevel < 0) fireEmissionLevel = 0;

            SetEmission(_fire, (int) (FireEmissionRateMax * fireEmissionLevel));

            int smokeEmissionRate = _fireLevel == 0 ? 0 : 1;
            SetEmission(_smoke1, smokeEmissionRate, Smoke1DurationMax * _fireLevel);
            SetEmission(_smoke2, smokeEmissionRate, Smoke2DurationMax * _fireLevel);
            SetEmission(_smoke3, smokeEmissionRate, Smoke3DurationMax * _fireLevel);
            SetEmission(_smoke4, smokeEmissionRate, Smoke4DurationMax * _fireLevel);

            _fireLightImage.color = Color.Lerp(Color.black, Color.white, _fireLevel);
        }

        public static void Tend()
        {
            if (!_tending) _crackling.FadeIn(6f);
            _tending = true;
            _fireLevel += 0.333f;
            if (_fireLevel > 1) _fireLevel = 1;
            _instance.UpdateRates();
        }

        public static void FinishTending()
        {
            _tending = false;
        }

        public static void Die()
        {
            if (_tending) return;
            _fireLevel -= 0.02f;
            if (_fireLevel < 0) _fireLevel = 0;
            _instance.UpdateRates();
            if (_fireLevel > 0) return;
            _crackling.FadeOut(1f);
            _fireLevel = 0;
        }

        public static bool IsLit() => _fireLevel > FireBurnOutPoint;

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Campfire");
            root.CreateChild("FireLevel", _fireLevel);
        }

        public static void Load(XmlNode root)
        {
            root = root.GetNode("Campfire");
            _fireLevel = root.FloatFromNode("FireLevel");
        }
    }
}