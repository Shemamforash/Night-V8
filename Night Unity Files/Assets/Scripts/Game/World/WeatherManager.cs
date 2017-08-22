﻿using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Game.World
{
    public class WeatherManager : StateMachine
    {
        private static WeatherManager _self;
        private TimeListener _timeListener = new TimeListener();
        private Text _weatherText;

        public void Awake()
        {
            base.Awake();
            _self = this;
            _weatherText = GameObject.Find("Weather").transform.Find("Text").GetComponent<Text>();
        }

        public void Start()
        {
            LoadWeather();
            LoadWeatherProbabilities();
            NavigateToState("Clear");
            _timeListener.OnMinute(() => ((Weather) GetCurrentState()).UpdateWeather());
//            GenerateWeatherString(2000);
        }

        public override void NavigateToState(string stateName)
        {
            base.NavigateToState(stateName);
            _weatherText.text = stateName;
        }

        public static WeatherManager GetWeatherManager()
        {
            return _self;
        }

        private void LoadWeather()
        {
            List<string> lines = Helper.ReadLinesFromFile("WeatherBalance");
            foreach (string line in lines)
            {
                string[] attributes = line.Split(',');
                string weatherName = attributes[0];
                float temperatureMin = float.Parse(attributes[1]);
                float temperatureMax = float.Parse(attributes[2]);
                MyFloat temperature = new MyFloat(temperatureMax, temperatureMin, temperatureMax);
                float visibilityMin = float.Parse(attributes[3]);
                float visibilityMax = float.Parse(attributes[4]);
                MyFloat visibility = new MyFloat(visibilityMax, visibilityMin, visibilityMax);
                float waterMin = float.Parse(attributes[5]);
                float waterMax = float.Parse(attributes[6]);
                MyFloat water = new MyFloat(waterMax, waterMin, waterMax);
                float durationMin = float.Parse(attributes[7]);
                float durationMax = float.Parse(attributes[8]);
                MyFloat duration = new MyFloat(durationMax, durationMin, durationMax);
                Weather weather = new Weather(weatherName, temperature, visibility, water, duration);
                AddState(weather);
                if (attributes[9] != "0")
                {
                    weather.AddDanger(attributes[9], float.Parse(attributes[10]));
                }
                if (attributes[11] != "0")
                {
                    weather.AddDanger(attributes[11], float.Parse(attributes[12]));
                }
            }
        }

        private void LoadWeatherProbabilities()
        {
            List<string> lines = Helper.ReadLinesFromFile("WeatherProbabilityTable");
            string[] weatherOrder = lines[0].Split(',');
            for (int i = 1; i < lines.Count; ++i)
            {
                string[] probabilities = lines[i].Split(',');
                Weather weather = (Weather) _states[probabilities[0]];
                for (int j = 1; j < probabilities.Length; ++j)
                {
                    weather.AddWeatherProbability(weatherOrder[j], float.Parse(probabilities[j]));
                }
            }
        }

        private void GenerateWeatherString(int stringLength)
        {
            string weatherString = "";
            Dictionary<string, int> _weatherOccurences = new Dictionary<string, int>();
            while (stringLength > 0)
            {
                string currentWeatherName = GetCurrentState().Name();
                if (_weatherOccurences.ContainsKey(currentWeatherName))
                {
                    _weatherOccurences[currentWeatherName]++;
                }
                else
                {
                    _weatherOccurences[currentWeatherName] = 1;
                }
                weatherString += currentWeatherName;
                GetCurrentState().Exit();
                --stringLength;
            }
            foreach (string key in _weatherOccurences.Keys)
            {
                Debug.Log(key + " occured " + _weatherOccurences[key] + " times.");
            }
            Debug.Log(weatherString);
        }
    }
}