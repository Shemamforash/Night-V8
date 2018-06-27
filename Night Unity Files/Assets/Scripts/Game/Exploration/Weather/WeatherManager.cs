using System.Collections.Generic;
using System.Xml;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Weather
{
    public static class WeatherManager
    {
        private static ProbabalisticStateMachine _weatherStates = new ProbabalisticStateMachine();
        private static bool _loaded;

        public static void Start()
        {
            LoadWeather();
            _weatherStates.LoadProbabilities("WeatherProbabilityTable");
            _weatherStates.GetState("Clear").Enter();
//            GenerateWeatherString(2000);
        }

        public static void GoToWeather()
        {
            _weatherStates.GetState(_weatherStates.CalculateNextState()).Enter();
        }

        public static Weather CurrentWeather()
        {
            return (Weather) _weatherStates.GetCurrentState();
        }

        private static void LoadWeather()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Weather", "WeatherTypes");
            foreach (XmlNode weatherNode in root.SelectNodes("Weather"))
            {
                string name = weatherNode.SelectSingleNode("Name").InnerText;
                string type = weatherNode.SelectSingleNode("Type").InnerText;
                int temperature = int.Parse(weatherNode.SelectSingleNode("Temperature").InnerText);
                float visibility = float.Parse(weatherNode.SelectSingleNode("Visibility").InnerText);
                float water = float.Parse(weatherNode.SelectSingleNode("Water").InnerText);
                float food = float.Parse(weatherNode.SelectSingleNode("Food").InnerText);
                int duration = int.Parse(weatherNode.SelectSingleNode("Duration").InnerText);
                XmlNode particleNode = weatherNode.SelectSingleNode("Particles");
                float rain = float.Parse(particleNode.SelectSingleNode("Rain").InnerText);
                float fog = float.Parse(particleNode.SelectSingleNode("Fog").InnerText);
                float dust = float.Parse(particleNode.SelectSingleNode("Dust").InnerText);
                float hail = float.Parse(particleNode.SelectSingleNode("Hail").InnerText);
                float sun = float.Parse(particleNode.SelectSingleNode("Sun").InnerText);
                Weather weather = new Weather(_weatherStates, name, temperature, visibility, water, food, duration);
                if (type == "Phenomena") _weatherStates.AddState(weather);
                weather.Attributes = new WeatherAttributes(rain, fog, dust, hail, sun);
            }

            _loaded = true;
        }

        private static void GenerateWeatherString(int stringLength)
        {
            string weatherString = "";
            Dictionary<string, int> _weatherOccurences = new Dictionary<string, int>();
            while (stringLength > 0)
            {
                string currentWeatherName = CurrentWeather().Name;
                if (_weatherOccurences.ContainsKey(currentWeatherName))
                    _weatherOccurences[currentWeatherName]++;
                else
                    _weatherOccurences[currentWeatherName] = 1;
                weatherString += currentWeatherName;
                CurrentWeather().Exit();
                --stringLength;
            }

            foreach (string key in _weatherOccurences.Keys) Debug.Log(key + " occured " + _weatherOccurences[key] + " times.");
            Debug.Log(weatherString);
        }
    }
}