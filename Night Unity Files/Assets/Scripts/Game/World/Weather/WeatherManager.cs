using System.Collections.Generic;
using Game.World.Time;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.CustomTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.Weather
{
    public class WeatherManager : ProbabalisticStateMachine
    {
        private static WeatherManager _instance;
        private TextMeshProUGUI _weatherText;

        public void Awake()
        {
            _instance = this;
            _weatherText = GameObject.Find("Weather").GetComponent<TextMeshProUGUI>();
        }

        public void Start()
        {
            LoadWeather();
            LoadProbabilities("WeatherProbabilityTable");
            NavigateToState("Clear");
            WorldTime.Instance().MinuteEvent += () => ((Weather) GetCurrentState()).UpdateWeather();
//            GenerateWeatherString(2000);
        }

        public override State NavigateToState(string stateName)
        {
            _weatherText.text = stateName;
            return base.NavigateToState(stateName);
        }

        public static WeatherManager Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }
            return FindObjectOfType<WeatherManager>();
        }

        private void LoadWeather()
        {
            Helper.ConstructObjectsFromCsv("WeatherBalance", delegate(string[] attributes)
            {
                string weatherName = attributes[0];
                int temperatureMin = int.Parse(attributes[1]);
                int temperatureMax = int.Parse(attributes[2]);
                MyInt temperature = new MyInt(temperatureMax, temperatureMin, temperatureMax);
                int visibilityMin = int.Parse(attributes[3]);
                int visibilityMax = int.Parse(attributes[4]);
                MyInt visibility = new MyInt(visibilityMax, visibilityMin, visibilityMax);
                int waterMin = int.Parse(attributes[5]);
                int waterMax = int.Parse(attributes[6]);
                MyInt water = new MyInt(waterMax, waterMin, waterMax);
                int durationMin = int.Parse(attributes[7]);
                int durationMax = int.Parse(attributes[8]);
                MyInt duration = new MyInt(durationMax, durationMin, durationMax);
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
                weather.Attributes = new WeatherAttributes(rainAmount, fogAmount, dustAmount, hailAmount);
            });
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