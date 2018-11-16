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
        private ParticleSystem _sun;
        private ParticleSystem _stars;
        [SerializeField] private float _fogMax;
        [SerializeField] private float _hailMax;
        [SerializeField] private float _rainMax;
        [SerializeField] private float _dustMax;
        [SerializeField] private float _windMax;
        private const float StarsMax = 150;
        [SerializeField] [Range(0, 1)] private float _sunMinBrightness, _sunMaxBrightness;
        private float _time;
        private static Weather _currentWeather;

        public void Awake()
        {
            _fog = new FogSystem(gameObject.FindChildWithName("Fog"), _fogMax);
            _rain = new RainSystem(gameObject.FindChildWithName("Rain"), _rainMax);
            _hail = new WeatherSystem(gameObject.FindChildWithName("Hail"), _hailMax);
            _dust = new WeatherSystem(gameObject.FindChildWithName("Dust"), _dustMax);
            _wind = new WindSystem(gameObject.FindChildWithName("Wind"), _windMax);
            _sun = gameObject.FindChildWithName<ParticleSystem>("Sun");
            _stars = gameObject.FindChildWithName<ParticleSystem>("Stars");
            _instance = this;
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

        private void UpdateSun()
        {
            float sunLevel = (float) (-0.014f * Math.Pow(_time - 12, 2) + 1f);
            sunLevel = Mathf.Clamp(sunLevel, 0, 1);
            float ambientVolume = _time < 6 || _time > 18 ? 0.5f : sunLevel;
            AudioController.SetAmbientVolume(ambientVolume);
            float minBrightness = _sunMinBrightness * sunLevel;
            float maxBrightness = _sunMaxBrightness * sunLevel;
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