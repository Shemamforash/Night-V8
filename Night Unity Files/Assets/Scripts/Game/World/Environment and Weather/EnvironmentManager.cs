using System.Collections.Generic;
using Game.World.Region;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.StateMachines;
using TMPro;
using UnityEngine;

namespace Game.World.Environment_and_Weather
{
    public class EnvironmentManager : StateMachine<Environment>
    {
        private TextMeshProUGUI _environmentText, _temperatureText;
        private static EnvironmentManager _instance;

        public void Awake()
        {
            _instance = this;
        }

        public static EnvironmentManager Instance()
        {
            Assert.NotNull(_instance);
            return _instance;
        }

        public void Start()
        {
            _environmentText = GameObject.Find("Environment").GetComponent<TextMeshProUGUI>();
            _temperatureText = GameObject.Find("Temperature").GetComponent<TextMeshProUGUI>();
            LoadEnvironments();
            NavigateToState("Oasis");
            WorldState.RegisterTravelEvent(GenerateEnvironment);
            WorldState.RegisterMinuteEvent(UpdateTemperature);
//            TestEnvironmentGenerator();
        }

        public override Environment NavigateToState(string stateName)
        {
            Environment newState = base.NavigateToState(stateName);
            RegionManager.GenerateNewRegions();
            _environmentText.text = GetCurrentState().Name;
            return newState;
        }

        private void TestEnvironmentGenerator()
        {
            int expectedTransitions = StatesAsList().Count;
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

        private void LoadEnvironments()
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

                Environment environment = new Environment(name, temperature, climate, waterAbundance, foodAbundance, fuelAbundance, scrapAbundance);
                AddState(environment);
            });
            SetDefaultState("Oasis");
        }

        private void UpdateTemperature()
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
            _temperatureText.text = temperatureCategory + seperators + "(" + currentTemperature  + ")";
        }

        public int GetTemperature()
        {
            return GetCurrentState().GetTemperature() + WeatherManager.Instance().GetCurrentState().Temperature();
        }

        private void GenerateEnvironment()
        {
            NavigateToState(GetCurrentState().Name);
        }
    }
}