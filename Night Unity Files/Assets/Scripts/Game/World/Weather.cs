using System;
using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.World
{
    public class Weather : State
    {
        private MyFloat _temperature, _visibility, _water, _duration;
        private List<Danger> _dangers = new List<Danger>();
        private Dictionary<string, float> probabilityDictionary = new Dictionary<string, float>();
        private int _timeRemaining;
        
        public Weather(string name, MyFloat temperature, MyFloat visibility, MyFloat water, MyFloat duration) : base(name, WeatherManager.GetWeatherManager())
        {
            _temperature = temperature;
            _visibility = visibility;
            _water = water;
            _duration = duration;
        }

        public void AddDanger(string dangerType, float severity)
        {
            _dangers.Add(new Danger(dangerType, severity));
        }

        public void AddWeatherProbability(string weatherName, float probability)
        {
            probabilityDictionary[weatherName] = probability;
        }

        public override void Enter()
        {
            _timeRemaining = (int)(_duration.RandomInRange() * WorldTime.MinutesPerHour);
        }

        public void UpdateWeather()
        {
            --_timeRemaining;
            if (_timeRemaining == 0)
            {
                Exit();
            }
        }

        public override void Exit()
        {
            WeatherManager.GetWeatherManager().NavigateToState(NextWeather(this));
        }

        public string NextWeather(Weather lastWeather)
        {
            string lastWeatherName = lastWeather == null ? "" : lastWeather.Name();
            float cumulativeValue = 0;
            float targetValue = UnityEngine.Random.Range(0f, 1.0f);
            string fallBackWeather = "";
            foreach (string weatherName in probabilityDictionary.Keys)
            {
                cumulativeValue += probabilityDictionary[weatherName];
                if (cumulativeValue >= targetValue)
                {
                    if (weatherName != lastWeatherName)
                    {
                        return weatherName;
                    }
                }
                else
                {
                    fallBackWeather = weatherName;
                }
            }
            return fallBackWeather;
        }

        private class Danger
        {
            private enum DangerType
            {
                Acid,
                Physical,
                Mental,
                Lightning,
                Fire,
                Cold
            }

            private DangerType _dangerType;
            private float _severity;

            public Danger(string dangerType, float severity)
            {
                foreach (DangerType type in Enum.GetValues(typeof(DangerType)))
                {
                    if (type.ToString() == dangerType)
                    {
                        _dangerType = type;
                        break;
                    }
                }
                _severity = severity;
            }
        }
    }
}