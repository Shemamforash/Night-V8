using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Weather;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public static class EnvironmentManager
    {
        private static bool _loaded;
        private static readonly Dictionary<int, Environment> _environments = new Dictionary<int, Environment>();
        private static Environment _currentEnvironment;
        private static TemperatureCategory _temperatureCategory;

        public static void Start()
        {
            UpdateTemperature();
            WorldView.SetEnvironmentText(_currentEnvironment.EnvironmentType.ToString());
            SceneryController.UpdateEnvironmentBackground();
        }

        public static void Reset(bool isLoading)
        {
            NextLevel(true, isLoading);
        }

        public static Environment CurrentEnvironment => _currentEnvironment;

        public static void NextLevel(bool reset, bool isLoading)
        {
            LoadEnvironments();
            if (reset) _currentEnvironment = _environments[WorldState._currentLevel - 1];
            else
            {
                int nextEnvironmentIndex = _currentEnvironment.LevelNo + 1;
                Assert.IsTrue(_environments.ContainsKey(nextEnvironmentIndex));
                _currentEnvironment = _environments[nextEnvironmentIndex];
            }

            if (!isLoading) MapGenerator.Generate();
        }

        private static void LoadEnvironments()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Environments", "EnvironmentTypes");
            foreach (XmlNode environmentNode in root.ChildNodes)
            {
                Environment environment = new Environment(environmentNode);
                _environments.Add(environment.LevelNo, environment);
            }

            _loaded = true;
        }

        public static void UpdateTemperature()
        {
            int temperature = CalculateTemperature();
            if (temperature < -20)
                _temperatureCategory = TemperatureCategory.Freezing;
            else if (temperature < 0)
                _temperatureCategory = TemperatureCategory.Cold;
            else if (temperature < 20)
                _temperatureCategory = TemperatureCategory.Warm;
            else if (temperature < 40)
                _temperatureCategory = TemperatureCategory.Hot;
            else
                _temperatureCategory = TemperatureCategory.Burning;

            WorldView.SetTemperatureText(_temperatureCategory.ToString());
        }

        public static TemperatureCategory GetTemperature()
        {
            return _temperatureCategory;
        }

        private static int CalculateTemperature()
        {
            Environment currentEnvironment = _currentEnvironment;
            Weather.Weather currentWeather = WeatherManager.CurrentWeather();
            return currentEnvironment.GetTemperature() + currentWeather.Temperature();
        }

        public static bool BelowFreezing()
        {
            return _temperatureCategory == TemperatureCategory.Cold || _temperatureCategory == TemperatureCategory.Freezing;
        }

        public static void Save(XmlNode doc)
        {
            if (_currentEnvironment == null) return;
            doc.CreateChild("CurrentEnvironment", _currentEnvironment.EnvironmentType.ToString());
        }

        public static void Load(XmlNode doc)
        {
            string currentEnvironmentText = doc.StringFromNode("CurrentEnvironment");
            foreach (KeyValuePair<int, Environment> environment in _environments)
            {
                if (environment.Value.ToString() != currentEnvironmentText) continue;
                _currentEnvironment = environment.Value;
                return;
            }
        }
    }
}