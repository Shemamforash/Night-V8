using System;
using System.Collections;
using Facilitating;
using Game.Global;
using UnityEngine;

namespace Game.Exploration.Weather
{
    public class WeatherSystemController : MonoBehaviour
    {
        private static WeatherSystemController _instance;
        private float _changeTime;
        private WeatherAttributes _prevAttributes, _targetAttributes;
        public float DustMax;
        public ParticleSystem Fog, Rain, Hail, Dust;
        public float FogMax;
        public float HailMax;
        public float RainMax;

        public static WeatherSystemController Instance()
        {
            if (_instance == null) return FindObjectOfType<WeatherSystemController>();
            return _instance;
        }

        public void ChangeWeather(Weather w, int minutes)
        {
            float time = minutes * WorldState.MinuteInSeconds;
            _changeTime = time / 10;
            _prevAttributes = _targetAttributes;
            _targetAttributes = w.Attributes;
            if (_prevAttributes == null)
            {
                SetFog(_targetAttributes.FogAmount);
                SetRain(_targetAttributes.RainAmount);
                SetDust(_targetAttributes.DustAmount);
                SetHail(_targetAttributes.HailAmount);
            }
            else
            {
                StartCoroutine(nameof(UpdateWeather));
            }
        }


        private IEnumerator UpdateWeather()
        {
            float startTime = Time.time;
            float targetTime = startTime + _changeTime;
            float totalTime = targetTime - startTime;
            while (Time.time < targetTime)
            {
                float percentComplete = 1 / totalTime * (Time.time - startTime);
                SunController.SetWeatherModifier(_targetAttributes.SunAmount);
                SetFog(CalculateWeatherAmount(percentComplete, _prevAttributes.FogAmount, _targetAttributes.FogAmount));
                SetRain(CalculateWeatherAmount(percentComplete, _prevAttributes.RainAmount, _targetAttributes.RainAmount));
                SetHail(CalculateWeatherAmount(percentComplete, _prevAttributes.HailAmount, _targetAttributes.HailAmount));
                SetDust(CalculateWeatherAmount(percentComplete, _prevAttributes.DustAmount, _targetAttributes.DustAmount));
                yield return null;
            }
        }

        private float CalculateWeatherAmount(float percent, float prevAmount, float targetAmount)
        {
            return (targetAmount - prevAmount) * percent + prevAmount;
        }

        private void SetWeather(float amount, float max, ParticleSystem particleSystem, Action<float> weatherChangeAction)
        {
            amount *= max;
            if (Math.Abs(amount) < 0.001)
                particleSystem.Stop();
            else
                particleSystem.Play();
            weatherChangeAction(amount);
        }

        private void SetFog(float amount)
        {
            amount /= 255f;
            SetWeather(amount, FogMax, Fog, f =>
            {
                ParticleSystem.MainModule mainModule = Fog.main;
                float value = 0.3f;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(new Color(value, value, value, 0), new Color(value, value, value, f));
            });
        }

        private void SetRain(float amount)
        {
            SetWeather(amount, RainMax, Rain, f =>
            {
                ParticleSystem.EmissionModule emissionModule = Rain.emission;
                emissionModule.rateOverTime = f;
            });
        }

        private void SetHail(float amount)
        {
            SetWeather(amount, HailMax, Hail, f =>
            {
                ParticleSystem.EmissionModule emissionModule = Hail.emission;
                emissionModule.rateOverTime = f;
            });
        }

        private void SetDust(float amount)
        {
            SetWeather(amount, DustMax, Dust, f =>
            {
                ParticleSystem.EmissionModule emissionModule = Dust.emission;
                emissionModule.rateOverTime = f;
            });
        }
    }
}