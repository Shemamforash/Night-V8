using System.Collections.Generic;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.StateMachines;
using TMPro;
using UnityEngine;

namespace Game.World.Environment_and_Weather
{
    public class EnvironmentManager : ProbabalisticStateMachine<Environment>
    {
        private TextMeshProUGUI _environmentText, _temperatureText;

        public void Start()
        {
            _environmentText = GameObject.Find("Environment").GetComponent<TextMeshProUGUI>();
            _temperatureText = GameObject.Find("Temperature").GetComponent<TextMeshProUGUI>();
            VisitAllStates();
            LoadEnvironments();
            LoadProbabilities("EnvironmentProbabilityTable");
            NavigateToState("Oasis");
            WorldState.RegisterTravelEvent(GenerateEnvironment);
            WorldState.RegisterMinuteEvent(UpdateTemperature);
            OnlyVisitOnce();
//            TestEnvironmentGenerator();
        }

        public override Environment NavigateToState(string stateName)
        {
            Environment newState = base.NavigateToState(stateName);
            RegionManager.GenerateNewRegions();
            _environmentText.text = GetCurrentState().GetDisplayName();
            return newState;
        }

        private void TestEnvironmentGenerator()
        {
            int expectedTransitions = StatesAsList().Count;
            int fails = 0;
            const int trials = 10000;
            for (int i = 0; i < trials; ++i)
            {
                ClearVisitedStates();
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
                Environment environment = new Environment(
                    attributes[0],
                    attributes[1],
                    this,
                    float.Parse(attributes[2]),
                    float.Parse(attributes[3]),
                    float.Parse(attributes[4]),
                    float.Parse(attributes[5]),
                    float.Parse(attributes[6]),
                    float.Parse(attributes[7])
                );
                AddState(environment);
            });
        }

        private void UpdateTemperature()
        {
            _temperatureText.text = GetTemperature() + "\u00B0" + "C";
        }

        public int GetTemperature()
        {
            return GetCurrentState().GetTemperature() + WeatherManager.Instance().GetCurrentState().Temperature();
        }

        private void GenerateEnvironment()
        {
            NavigateToState(GetCurrentState().Name);
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                GenerateEnvironment();
            }
        }
#endif
    }
}