﻿using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Exploration.Weather;
using Game.Global;
using SamsHelper.Libraries;

namespace Game.Exploration.Environment
{
    public static class EnvironmentManager
    {
        private static bool _loaded;
        private static readonly Dictionary<EnvironmentType, Environment> _environments = new Dictionary<EnvironmentType, Environment>();
        private static Environment _currentEnvironment;
        private static TemperatureCategory _temperatureCategory;
        private static EnvironmentType _currentEnvironmentType;

        public static void Start()
        {
            UpdateTemperature();
            WorldView.SetEnvironmentText(Environment.EnvironmentTypeToName(_currentEnvironment.EnvironmentType));
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
            if (reset) _currentEnvironmentType = EnvironmentType.Desert;
            else
            {
                switch (_currentEnvironment.EnvironmentType)
                {
                    case EnvironmentType.Desert:
                        _currentEnvironmentType = EnvironmentType.Mountains;
                        break;
                    case EnvironmentType.Mountains:
                        _currentEnvironmentType = EnvironmentType.Sea;
                        break;
                    case EnvironmentType.Sea:
                        _currentEnvironmentType = EnvironmentType.Ruins;
                        break;
                    case EnvironmentType.Ruins:
                        _currentEnvironmentType = EnvironmentType.Wasteland;
                        break;
                    case EnvironmentType.Wasteland:
                        _currentEnvironmentType = EnvironmentType.End;
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            SetCurrentEnvironment();
            if (!isLoading && _currentEnvironment != null) MapGenerator.Generate();
        }

        private static void SetCurrentEnvironment()
        {
            LoadEnvironments();
            if (_currentEnvironmentType == EnvironmentType.End)
            {
                _currentEnvironment = null;
                return;
            }

            _currentEnvironment = _environments[_currentEnvironmentType];
        }

        public static EnvironmentType CurrentEnvironmentType() => _currentEnvironmentType;

        private static void LoadEnvironments()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Environments", "EnvironmentTypes");
            foreach (XmlNode environmentNode in root.ChildNodes)
            {
                Environment environment = new Environment(environmentNode);
                _environments.Add(environment.EnvironmentType, environment);
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
            doc.CreateChild("CurrentEnvironment", _currentEnvironmentType.ToString());
        }

        public static void Load(XmlNode doc)
        {
            LoadEnvironments();
            string currentEnvironmentText = doc.StringFromNode("CurrentEnvironment");
            foreach (EnvironmentType environmentType in Enum.GetValues(typeof(EnvironmentType)))
            {
                if (environmentType.ToString() != currentEnvironmentText) continue;
                _currentEnvironmentType = environmentType;
                SetCurrentEnvironment();
                break;
            }
        }
    }
}