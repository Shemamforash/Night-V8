using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;

namespace Game.World.Environment_and_Weather
{
    public class WeatherManager : ProbabalisticStateMachine<Weather>
    {
        private static WeatherManager _instance;
        private static TextMeshProUGUI _weatherText;

        public WeatherManager()
        {
            _instance = this;
        }

        public void Start()
        {
            _weatherText = GameObject.Find("Weather").GetComponent<TextMeshProUGUI>();
            LoadWeather();
            LoadProbabilities("WeatherProbabilityTable");
            NavigateToState("Clear");
            WorldState.RegisterMinuteEvent(() => GetCurrentState().UpdateWeather());
//            GenerateWeatherString(2000);
        }

        public override Weather NavigateToState(string stateName)
        {
            _weatherText.text = stateName;
            return base.NavigateToState(stateName);
        }

        public static WeatherManager Instance()
        {
            return _instance;
        }

        private void LoadWeather()
        {
            Helper.ConstructObjectsFromCsv("WeatherBalance", delegate(string[] attributes)
            {
                string weatherName = attributes[0];
                int temperatureMin = int.Parse(attributes[1]);
                int temperatureMax = int.Parse(attributes[2]);
                MyValue temperature = new MyValue(temperatureMax, temperatureMin, temperatureMax);
                int visibilityMin = int.Parse(attributes[3]);
                int visibilityMax = int.Parse(attributes[4]);
                MyValue visibility = new MyValue(visibilityMax, visibilityMin, visibilityMax);
                int waterMin = int.Parse(attributes[5]);
                int waterMax = int.Parse(attributes[6]);
                MyValue water = new MyValue(waterMax, waterMin, waterMax);
                int durationMin = int.Parse(attributes[7]);
                int durationMax = int.Parse(attributes[8]);
                MyValue duration = new MyValue(durationMax, durationMin, durationMax);
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
                float rainAmount = float.Parse(attributes[13]);
                float fogAmount = float.Parse(attributes[14]);
                float dustAmount = float.Parse(attributes[15]);
                float hailAmount = float.Parse(attributes[16]);
                float sunAmount = float.Parse(attributes[17]);
                weather.Attributes = new WeatherAttributes(rainAmount, fogAmount, dustAmount, hailAmount, sunAmount);
            });
        }

        private void GenerateWeatherString(int stringLength)
        {
            string weatherString = "";
            Dictionary<string, int> _weatherOccurences = new Dictionary<string, int>();
            while (stringLength > 0)
            {
                string currentWeatherName = GetCurrentState().Name;
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