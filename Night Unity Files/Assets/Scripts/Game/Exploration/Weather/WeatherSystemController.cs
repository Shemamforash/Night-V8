using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using Facilitating;
using Facilitating.Audio;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Game.Exploration.Weather
{
    public class WeatherSystemController : MonoBehaviour
    {
        private static WeatherSystemController _instance;
        private FogSystem _fog;
        private RainSystem _rain;
        private HailSystem _hail;
        private DustSystem _dust;
        private WindSystem _wind;
        private ParticleSystem _sun;
        private ParticleSystem _stars;
        [SerializeField] private float _fogMax;
        [SerializeField] private float _hailMax;
        [SerializeField] private float _rainMax;
        [SerializeField] private float _dustMax;
        [SerializeField] private float _windMax;
        [SerializeField] private float _starsMax;
        [SerializeField] [Range(0, 1)] private float _sunMinBrightness, _sunMaxBrightness;

        private static bool _audioLoaded;
        private static AudioClip[] _lightRainClips, _mediumRainClips, _heavyRainClips;
        private static AudioClip[] _lightWindClips, _mediumWindClips, _heavyWindClips;
        private AudioSource _nightTimeAudioSource, _dayTimeAudioSource;

        public static AudioClip GetRainClip(float strength)
        {
            if (strength > 0.7f) return _heavyRainClips.RandomElement();
            if (strength > 0.4) return _mediumRainClips.RandomElement();
            return strength > 0 ? _lightRainClips.RandomElement() : null;
        }

        public static AudioClip GetWindClip(float strength)
        {
            if (strength > 0.7f) return _heavyWindClips.RandomElement();
            if (strength > 0.4) return _mediumWindClips.RandomElement();
            return strength > 0 ? _lightWindClips.RandomElement() : null;
        }
        
        private static void LoadAudioClips()
        {
            if (_audioLoaded) return;
            _lightRainClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("rain/light");
            _mediumRainClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("rain/medium");
            _heavyRainClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("rain/heavy");
            _lightWindClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("wind/light");
            _mediumWindClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("wind/medium");
            _heavyWindClips = Helper.LoadAllFilesFromAssetBundle<AudioClip>("wind/heavy");
            _audioLoaded = true;
        }

        public void Awake()
        {
            LoadAudioClips();
            _fog = new FogSystem(gameObject.FindChildWithName("Fog"), _fogMax);
            _rain = new RainSystem(gameObject.FindChildWithName("Rain"), _rainMax);
            _hail = new HailSystem(gameObject.FindChildWithName("Hail"), _hailMax);
            _dust = new DustSystem(gameObject.FindChildWithName("Dust"), _dustMax);
            _wind = new WindSystem(gameObject.FindChildWithName("Wind"), _windMax);
            _sun = gameObject.FindChildWithName<ParticleSystem>("Sun");
            _stars = gameObject.FindChildWithName<ParticleSystem>("Stars");
            _nightTimeAudioSource = _stars.GetComponent<AudioSource>();
            _dayTimeAudioSource = _sun.GetComponent<AudioSource>();
            _instance = this;
            if (_currentWeather != null) ChangeWeatherInstant();
        }

        public static WeatherSystemController Instance()
        {
            return _instance == null ? FindObjectOfType<WeatherSystemController>() : _instance;
        }

        private float _time;
        private static Weather _currentWeather;

        public void Update()
        {
            UpdateTime();
            UpdateSun();
            UpdateStars();
        }

        private void UpdateStars()
        {
            ParticleSystem.EmissionModule starEmission = _stars.GetComponent<ParticleSystem>().emission;
            float time = WorldState.Hours;
            if (time > 6) time = Mathf.Abs(time - 24);
            float starEmissionRate = 1f - time / 6f;
            if (_time >= 6 && _time <= 18) starEmissionRate = 0f;
            _nightTimeAudioSource.volume = starEmissionRate;
            starEmission.rateOverTime = _starsMax * starEmissionRate;
            UpdateMoon();
        }

        private void UpdateTime()
        {
            float time = WorldState.Hours;
            float minutes = (float) WorldState.Minutes / WorldState.MinutesPerHour;
            minutes /= 5f;
            time += minutes;
            _time = time;
        }

        private void UpdateMoon()
        {
        }

        private void UpdateSun()
        {
            float timeOfDayModifier = (float) (-0.02f * Math.Pow(_time - 12, 2) + 1f);
            if (_time < 6 || _time > 18) timeOfDayModifier = 0f;
            _dayTimeAudioSource.volume = timeOfDayModifier;
            float minBrightness = _sunMinBrightness * timeOfDayModifier;
            float maxBrightness = _sunMaxBrightness * timeOfDayModifier;
            ParticleSystem.MainModule sunMain = _sun.GetComponent<ParticleSystem>().main;
            sunMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, maxBrightness), new Color(1, 1, 1, minBrightness));
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
            StartCoroutine(_rain.ChangeWeather(_targetAttributes.RainAmount));
            StartCoroutine(_dust.ChangeWeather(_targetAttributes.DustAmount));
            StartCoroutine(_wind.ChangeWeather(_targetAttributes.WindAmount));
            StartCoroutine(_hail.ChangeWeather(_targetAttributes.HailAmount));
            StartCoroutine(_fog.ChangeWeather(_targetAttributes.RainAmount));
        }

        private void ChangeWeatherInstant()
        {
            WeatherAttributes _targetAttributes = _currentWeather.Attributes;
            _rain.ChangeWeatherInstant(_targetAttributes.RainAmount);
            _dust.ChangeWeatherInstant(_targetAttributes.DustAmount);
            _wind.ChangeWeatherInstant(_targetAttributes.WindAmount);
            _hail.ChangeWeatherInstant(_targetAttributes.HailAmount);
            _fog.ChangeWeatherInstant(_targetAttributes.FogAmount);
        }

        private class RainSystem : WeatherSystem
        {
            public RainSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                return GetRainClip(amount);
            }
        }

        private class WindSystem : WeatherSystem
        {
            public WindSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                return GetWindClip(amount);
            }
        }

        private class HailSystem : WeatherSystem
        {
            public HailSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                return null;
            }
        }

        private class FogSystem : WeatherSystem
        {
            private AudioMixer _audioMixer;

            public FogSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
                _audioMixer.SetFloat("WeatherEcho", amount * 50f);
                _audioMixer.SetFloat("WeatherWetMix", amount * 100f);
                _audioMixer.SetFloat("WeatherCutoffFreq", 22000 - amount * 20000f);
                return null;
            }
        }

        private class DustSystem : WeatherSystem
        {
            public DustSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                return null;
            }
        }

        private abstract class WeatherSystem
        {
            private readonly ParticleSystem _particles;
            private readonly float _maxEmission;
            private float _currentWeatherAmount;
            private readonly CrossFader _crossFader;

            protected WeatherSystem(GameObject weatherObject, float maxEmission)
            {
                _particles = weatherObject.GetComponent<ParticleSystem>();
                _crossFader = weatherObject.GetComponent<CrossFader>();
                _crossFader.SetLooping();
                _crossFader.StartAtRandomPosition();
                _crossFader.SetMixerGroup("Weather");
                _maxEmission = maxEmission;
            }

            protected abstract AudioClip GetAudioClipForWeather(float amount);

            public IEnumerator ChangeWeather(float amount)
            {
                ParticleSystem.EmissionModule emission = _particles.emission;
                float startRate = emission.rateOverTime.constant;
                float finalRate = amount * _maxEmission;

                AudioClip newClip = GetAudioClipForWeather(amount);
                _crossFader.CrossFade(newClip);
                float currentTime = 5f;
                while (currentTime > 0f)
                {
                    float normalisedTime = currentTime / 5f;
                    float newRate = Mathf.Lerp(startRate, finalRate, normalisedTime);
                    emission.rateOverTime = newRate;
                    currentTime -= Time.deltaTime;
                    yield return null;
                }

                emission.rateOverTime = finalRate;
            }

            public void ChangeWeatherInstant(float amount)
            {
                ParticleSystem.EmissionModule emission = _particles.emission;
                float finalRate = amount * _maxEmission;
                emission.rateOverTime = finalRate;
                AudioClip newClip = GetAudioClipForWeather(amount);
                _crossFader.CrossFade(newClip);
            }
        }
    }
}