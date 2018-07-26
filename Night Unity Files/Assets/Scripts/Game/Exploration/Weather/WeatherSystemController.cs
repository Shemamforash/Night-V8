using System;
using System.Collections;
using DG.Tweening;
using Facilitating;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
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

        [SerializeField] private AudioClip _rainLight, _rainMedium, _rainHeavy, _windHeavy;
        private AudioSource _nightTimeAudioSource, _dayTimeAudioSource;

        public void Awake()
        {
            _fog = new FogSystem(Helper.FindChildWithName(gameObject, "Fog"), _fogMax);
            _rain = new RainSystem(Helper.FindChildWithName(gameObject, "Rain"), _rainMax);
            _hail = new HailSystem(Helper.FindChildWithName(gameObject, "Hail"), _hailMax);
            _dust = new DustSystem(Helper.FindChildWithName(gameObject, "Dust"), _dustMax);
            _wind = new WindSystem(Helper.FindChildWithName(gameObject, "Wind"), _windMax);
            _sun = Helper.FindChildWithName<ParticleSystem>(gameObject, "Sun");
            _stars = Helper.FindChildWithName<ParticleSystem>(gameObject, "Stars");
            _nightTimeAudioSource = _stars.GetComponent<AudioSource>();
            _dayTimeAudioSource = _sun.GetComponent<AudioSource>();
            _instance = this;
        }

        public static WeatherSystemController Instance()
        {
            return _instance == null ? FindObjectOfType<WeatherSystemController>() : _instance;
        }

        private float _time;

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

        public void ChangeWeather(Weather w)
        {
            WeatherAttributes _targetAttributes = w.Attributes;
            StartCoroutine(_rain.ChangeWeather(_targetAttributes.RainAmount));
            StartCoroutine(_dust.ChangeWeather(_targetAttributes.DustAmount));
            StartCoroutine(_wind.ChangeWeather(_targetAttributes.WindAmount));
            StartCoroutine(_hail.ChangeWeather(_targetAttributes.HailAmount));
            StartCoroutine(_fog.ChangeWeather(_targetAttributes.FogAmount));
        }

        private class RainSystem : WeatherSystem
        {
            public RainSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                if (amount > 0.7f) return _instance._rainHeavy;
                if (amount > 0.4) return _instance._rainMedium;
                return amount > 0 ? _instance._rainLight : null;
            }
        }

        private class WindSystem : WeatherSystem
        {
            public WindSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                return amount > 0 ? _instance._windHeavy : null;
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

        private class SunSystem : WeatherSystem
        {
            public SunSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
                return null;
            }
        }

        private class FogSystem : WeatherSystem
        {
            public FogSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
            {
            }

            protected override AudioClip GetAudioClipForWeather(float amount)
            {
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

        private class StarSystem : WeatherSystem
        {
            public StarSystem(GameObject weatherObject, float maxEmission) : base(weatherObject, maxEmission)
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
            private readonly AudioSource _audioSourceA, _audioSourceB;

            private AudioSource _currentAudioSource;
            private AudioClip _currentClip;
            private float _currentWeatherAmount;

            protected WeatherSystem(GameObject weatherObject, float maxEmission)
            {
                _particles = weatherObject.GetComponent<ParticleSystem>();
                _audioSourceA = Helper.FindChildWithName<AudioSource>(weatherObject, "Audio A");
                _audioSourceB = Helper.FindChildWithName<AudioSource>(weatherObject, "Audio B");
                _maxEmission = maxEmission;
            }

            protected abstract AudioClip GetAudioClipForWeather(float amount);

            public IEnumerator ChangeWeather(float amount)
            {
                ParticleSystem.EmissionModule emission = _particles.emission;
                float startRate = emission.rateOverTime.constant;
                float finalRate = amount * _maxEmission;

                AudioClip newClip = GetAudioClipForWeather(amount);
                CrossFade(newClip);
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

            private void CrossFade(AudioClip to)
            {
                if (_currentAudioSource != null)
                {
                    if (_currentAudioSource.clip == to) return;
                    Sequence sequence = DOTween.Sequence();
                    AudioSource fadeOut = _currentAudioSource;
                    sequence.Append(fadeOut.DOFade(0, 2));
                    sequence.AppendCallback(() => fadeOut.Stop());
                }

                _currentAudioSource = _audioSourceA == _currentAudioSource ? _audioSourceB : _audioSourceA;
                _currentAudioSource.clip = to;
                if (to != null)
                {
                    float randomPosition = Random.Range(0f, to.length);
                    _currentAudioSource.time = randomPosition;
                }

                _currentAudioSource.volume = 0;
                _currentAudioSource.Play();
                _currentAudioSource.DOFade(1, 2);
            }
        }
    }
}