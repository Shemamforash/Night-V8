using System.Collections.Generic;
using Game.Exploration.Weather;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public static class EnvironmentManager
    {
        private static readonly StateMachine _environmentStates = new StateMachine();

        public static void Start()
        {
            LoadEnvironments();
            NavigateToState("Oasis");
//            TestEnvironmentGenerator();
        }

        private static void NavigateToState(string stateName)
        {
            _environmentStates.GetState(stateName).Enter();
            MapGenerator.Generate();
            WorldView.SetEnvironmentText(_environmentStates.GetCurrentState().Name);
        }

        private static void TestEnvironmentGenerator()
        {
            int expectedTransitions = _environmentStates.StatesAsList().Count;
            int fails = 0;
            const int trials = 10000;
            for (int i = 0; i < trials; ++i)
            {
                NavigateToState("Oasis");
                int totalTransitions = 1;
                try
                {
                    while (true)
                    {
                        GenerateEnvironment();
                        ++totalTransitions;
                    }
                }
                catch (KeyNotFoundException e)
                {
                    if (totalTransitions == expectedTransitions) continue;
                    ++fails;
                }
            }

            Debug.Log(fails + " failed tests from " + trials + " trials.");
        }

        private static void LoadEnvironments()
        {
            Helper.ConstructObjectsFromCsv("EnvironmentBalance", delegate(string[] attributes)
            {
                string name = attributes[0];
                int temperature = int.Parse(attributes[1]);
                int climate = int.Parse(attributes[2]);
                int waterAbundance = int.Parse(attributes[3]);
                int foodAbundance = int.Parse(attributes[4]);
                int fuelAbundance = int.Parse(attributes[5]);
                int scrapAbundance = int.Parse(attributes[6]);

                Environment environment = new Environment(_environmentStates, name, temperature, climate, waterAbundance, foodAbundance, fuelAbundance, scrapAbundance);
                _environmentStates.AddState(environment);
            });
            _environmentStates.SetDefaultState(_environmentStates.GetState("Oasis"));
        }

        public static void UpdateTemperature()
        {
            int temperature = GetTemperature();
            TemperatureCategory temperatureCategory;
            if (temperature < -20)
                temperatureCategory = TemperatureCategory.Freezing;
            else if (temperature < 0)
                temperatureCategory = TemperatureCategory.Cold;
            else if (temperature < 20)
                temperatureCategory = TemperatureCategory.Warm;
            else if (temperature < 40)
                temperatureCategory = TemperatureCategory.Hot;
            else
                temperatureCategory = TemperatureCategory.Boiling;
            int targetLength = 6;
            string currentTemperature = GetTemperature() + "\u00B0" + "C";
            int lengthDifference = targetLength - currentTemperature.Length;
            string seperators = "";
            for (int i = 0; i < lengthDifference; ++i) seperators += " ";
            WorldView.SetTemperatureText(temperatureCategory + seperators + "(" + currentTemperature + ")");
        }

        public static int GetTemperature()
        {
            Environment currentEnvironment = (Environment) _environmentStates.GetCurrentState();
            Weather.Weather currentWeather = WeatherManager.CurrentWeather();
            return currentEnvironment.GetTemperature() + currentWeather.Temperature();
        }

        public static void GenerateEnvironment()
        {
            NavigateToState(_environmentStates.GetCurrentState().Name);
        }
    }
}