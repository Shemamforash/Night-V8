using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Weather;
using Game.Exploration.WorldEvents;
using Game.Global;
using SamsHelper.Libraries;

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
            LoadEnvironments();
            NextLevel();
        }

        public static void Reset()
        {
            _currentEnvironment = null;
        }
        
        public static Environment CurrentEnvironment => _currentEnvironment;

        public static void NextLevel()
        {
            if (_currentEnvironment == null)
            {
                _currentEnvironment = _environments[0];
            }
            else
            {
                int nextEnvironmentIndex = _currentEnvironment.LevelNo + 1;
                if (_environments.ContainsKey(nextEnvironmentIndex))
                {
                    _currentEnvironment = _environments[nextEnvironmentIndex];
                }
            }

            MapGenerator.Generate();
            WorldView.SetEnvironmentText(_currentEnvironment.EnvironmentType.ToString());
            SceneryController.UpdateEnvironmentBackground();
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
                _temperatureCategory = TemperatureCategory.Boiling;

            int targetLength = 6;
            string currentTemperature = CalculateTemperature() + "\u00B0" + "C";
            int lengthDifference = targetLength - currentTemperature.Length;
            string seperators = "";
            for (int i = 0; i < lengthDifference; ++i) seperators += " ";
            WorldView.SetTemperatureText(_temperatureCategory + seperators + "(" + currentTemperature + ")");
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
    }
}