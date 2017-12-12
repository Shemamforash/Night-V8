using System.Collections.Generic;
using System.Xml;
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
            TextAsset weatherFile = Resources.Load<TextAsset>("Weather");
            XmlDocument weatherXml = new XmlDocument();
            weatherXml.LoadXml(weatherFile.text);
            XmlNode root = weatherXml.SelectSingleNode("WeatherTypes");
            foreach (XmlNode weatherNode in root.SelectNodes("Weather"))
            {
                string name = weatherNode.SelectSingleNode("Name").InnerText;
                string type = weatherNode.SelectSingleNode("Type").InnerText;
                int temperature = int.Parse(weatherNode.SelectSingleNode("Temperature").InnerText);
                int visibility = int.Parse(weatherNode.SelectSingleNode("Visibility").InnerText);
                int water = int.Parse(weatherNode.SelectSingleNode("Water").InnerText);
                int food = int.Parse(weatherNode.SelectSingleNode("Food").InnerText);
                int duration = int.Parse(weatherNode.SelectSingleNode("Duration").InnerText);
                XmlNode particleNode = weatherNode.SelectSingleNode("Particles");
                float rain = float.Parse(particleNode.SelectSingleNode("Rain").InnerText);
                float fog = float.Parse(particleNode.SelectSingleNode("Fog").InnerText);
                float dust = float.Parse(particleNode.SelectSingleNode("Dust").InnerText);
                float hail = float.Parse(particleNode.SelectSingleNode("Hail").InnerText);
                float sun = float.Parse(particleNode.SelectSingleNode("Sun").InnerText);
                Weather weather = new Weather(name, temperature, visibility, water, food, duration);
                if (type == "Phenomena") AddState(weather);
                weather.Attributes = new WeatherAttributes(rain, fog, dust, hail, sun);
            }
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