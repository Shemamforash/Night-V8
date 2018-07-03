using System.Collections.Generic;
using System.Xml;
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
        private static TemperatureCategory _lastTemperatureCategory;

        public static void Start()
        {
            LoadEnvironments();
            NextLevel();
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
        }

        private static void LoadEnvironments()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Environments", "EnvironmentTypes");
            foreach (XmlNode environmentNode in root.ChildNodes)
            {
                string name = environmentNode.Name;
                int level = int.Parse(environmentNode.SelectSingleNode("Level").InnerText);
                int temperature = int.Parse(environmentNode.SelectSingleNode("Temperature").InnerText);
                int shelter = int.Parse(environmentNode.SelectSingleNode("Shelter").InnerText);
                int temples = int.Parse(environmentNode.SelectSingleNode("Temples").InnerText);
                int keys = int.Parse(environmentNode.SelectSingleNode("CompleteKeys").InnerText);
                int resources = int.Parse(environmentNode.SelectSingleNode("Resources").InnerText);
                int danger = int.Parse(environmentNode.SelectSingleNode("Danger").InnerText);
                Environment environment = new Environment(name, level, temperature, shelter, temples, keys, resources, danger);
                _environments.Add(level, environment);
            }

            _loaded = true;
        }

        public static void UpdateTemperature()
        {
            int temperature = CalculateTemperature();
            _lastTemperatureCategory = _temperatureCategory;
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
            if (_lastTemperatureCategory != _temperatureCategory)
            {
                WorldEventManager.GenerateEvent(new WorldEvent("It's " + _temperatureCategory));
            }

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
    }
}