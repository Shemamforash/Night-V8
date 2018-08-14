using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.WorldEvents;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Weather
{
    public static class WeatherManager
    {
        private static readonly ProbabalisticStateMachine _weatherStates = new ProbabalisticStateMachine();
        private static bool _loaded;

        public static void Start()
        {
            LoadWeather();
            _weatherStates.LoadProbabilities("WeatherProbabilityTable");
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
            foreach (XmlNode weatherNode in Helper.GetNodesWithName(root, "Weather"))
                new Weather(_weatherStates, weatherNode);
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

        public static void Reset()
        {
            LoadWeather();
            _weatherStates.GetState("Clear").Enter();
        }

        public static void Save(XmlNode doc)
        {
            CurrentWeather().Save(doc);
        }
    }
}