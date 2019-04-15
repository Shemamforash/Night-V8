using System;
using DG.Tweening;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Weather
{
    public class WeatherSystemController : MonoBehaviour
    {
        private static WeatherSystemController _instance;
        private WeatherSystem _fog, _rain, _hail, _dust, _wind;
        private ParticleSystem _sun, _stars, _lightning;
        private const float FogMax = 30f;
        private const float HailMax = 50f;
        private const float RainMax = 300f;
        private const float DustMax = 100f;
        private const float WindMax = 50f;
        private const float StarsMax = 150;
        private const float SunMinBrightness = 0.692f;
        private const float SunMaxBrightness = 1f;
        private float _time;
        private static Weather _currentWeather;
        private static float _sunLevel;

        public void Awake()
        {
            _fog = new FogSystem(gameObject.FindChildWithName("Fog"), FogMax);
            _rain = new RainSystem(gameObject.FindChildWithName("Rain"), RainMax);
            _hail = new HailSystem(gameObject.FindChildWithName("Hail"), HailMax);
            _dust = new WeatherSystem(gameObject.FindChildWithName("Dust"), DustMax);
            _wind = new WindSystem(gameObject.FindChildWithName("Wind"), WindMax);
            _sun = gameObject.FindChildWithName<ParticleSystem>("Sun");
            _stars = gameObject.FindChildWithName<ParticleSystem>("Stars");
            _lightning = gameObject.FindChildWithName<ParticleSystem>("Lightning Strikes");
            _instance = this;
        }

        public static void TriggerLightning()
        {
            if (_instance._lightning == null) return;
            _instance._lightning.Emit(1);
        }

        public void Start()
        {
            if (_currentWeather != null) ChangeWeatherInstant();
        }

        public void Update()
        {
            UpdateTime();
            UpdateSun();
            UpdateStars();
        }

        private void UpdateTime()
        {
            _time = WorldState.Hours;
            _time += WorldState.Minutes / 5f / WorldState.MinutesPerHour;
        }

        public static float SunLevel() => _sunLevel;

        private void UpdateSun()
        {
            _sunLevel = (float) (-0.014f * Math.Pow(_time - 12, 2) + 1f);
            _sunLevel = Mathf.Clamp(_sunLevel, 0, 1);
            float ambientVolume = _time < 6 || _time > 18 ? 0.5f : _sunLevel;
            AudioController.SetAmbientVolume(ambientVolume);
            float minBrightness = SunMinBrightness * _sunLevel;
            float maxBrightness = SunMaxBrightness * _sunLevel;
            ParticleSystem.MainModule sunMain = _sun.GetComponent<ParticleSystem>().main;
            sunMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, maxBrightness), new Color(1, 1, 1, minBrightness));
        }

        private void UpdateStars()
        {
            ParticleSystem.EmissionModule starEmission = _stars.GetComponent<ParticleSystem>().emission;
            float starLevel = _time;
            if (starLevel <= 6) starLevel = 1 - starLevel / 6f;
            else if (starLevel >= 18) starLevel = starLevel / 6f - 3f;
            else starLevel = 0;
            AudioController.SetNightVolume(starLevel);
            starEmission.rateOverTime = StarsMax * starLevel;
        }

        public static void SetWeather(Weather w, bool instant)
        {
            _currentWeather = w;
            if (_instance == null) return;
            if (instant) _instance.ChangeWeatherInstant();
            else _instance.ChangeWeather();
        }

        private void ChangeWeather()
        {
            WeatherAttributes _targetAttributes = _currentWeather.Attributes;
            _rain.ChangeWeather(_targetAttributes.RainAmount, false);
            _dust.ChangeWeather(_targetAttributes.DustAmount, false);
            _wind.ChangeWeather(_targetAttributes.WindAmount, false);
            _hail.ChangeWeather(_targetAttributes.HailAmount, false);
            _fog.ChangeWeather(_targetAttributes.FogAmount, false);
        }

        private void ChangeWeatherInstant()
        {
            WeatherAttributes _targetAttributes = _currentWeather.Attributes;
            _rain.ChangeWeather(_targetAttributes.RainAmount, true);
            _dust.ChangeWeather(_targetAttributes.DustAmount, true);
            _wind.ChangeWeather(_targetAttributes.WindAmount, true);
            _hail.ChangeWeather(_targetAttributes.HailAmount, true);
            _fog.ChangeWeather(_targetAttributes.FogAmount, true);
        }

        private class RainSystem : WeatherSystem
        {
            public RainSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            public override void ChangeWeather(float amount, bool instant)
            {
                base.ChangeWeather(amount, instant);
                float lightTarget = amount > 0f && amount < 0.4f ? 1f : 0f;
                float mediumTarget = amount >= 0.4f && amount < 0.7f ? 1f : 0f;
                float heavyTarget = amount >= 0.7f ? 1f : 0f;

                AudioController.FadeRainLight(lightTarget / 0.4f, Duration);
                AudioController.FadeRainMedium(mediumTarget / 0.7f, Duration);
                AudioController.FadeRainHeavy(heavyTarget, Duration);
            }
        }

        private class FogSystem : WeatherSystem
        {
            public FogSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            public override void ChangeWeather(float amount, bool instant)
            {
                base.ChangeWeather(amount, instant);
                AudioController.FadeFog(amount, Duration);
            }
        }

        private class HailSystem : WeatherSystem
        {
            public HailSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            public override void ChangeWeather(float amount, bool instant)
            {
                base.ChangeWeather(amount, instant);
                AudioController.FadeHail(amount, Duration);
            }
        }

        private class WindSystem : WeatherSystem
        {
            public WindSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            public override void ChangeWeather(float amount, bool instant)
            {
                base.ChangeWeather(amount, instant);
                float lightTarget = amount > 0f && amount < 0.4f ? 1f : 0f;
                float mediumTarget = amount >= 0.4f && amount < 0.7f ? 1f : 0f;
                float heavyTarget = amount >= 0.7f ? 1f : 0f;

                AudioController.FadeWindLight(lightTarget / 0.4f, Duration);
                AudioController.FadeWindMedium(mediumTarget / 0.7f, Duration);
                AudioController.FadeWindHeavy(heavyTarget, Duration);
            }
        }

        private class WeatherSystem
        {
            private readonly ParticleSystem _particles;
            private readonly float _maxEmission;
            private float _currentWeatherAmount;
            private const float ChangeDuration = 5f;
            protected float Duration;

            public WeatherSystem(GameObject weatherObject, float maxEmission)
            {
                _particles = weatherObject.GetComponent<ParticleSystem>();
                _maxEmission = maxEmission;
            }

            public virtual void ChangeWeather(float amount, bool instant)
            {
                ParticleSystem.EmissionModule emission = _particles.emission;
                float finalRate = amount * _maxEmission;
                Duration = instant ? 0f : ChangeDuration;
                DOTween.To(() => emission.rateOverTime.constant, f => emission.rateOverTime = f, finalRate, Duration).SetUpdate(UpdateType.Normal, true);
            }
        }
    }
}