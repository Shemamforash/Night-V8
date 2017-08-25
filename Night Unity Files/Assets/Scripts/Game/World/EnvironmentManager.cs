using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Game.World
{
    public class EnvironmentManager : ProbabalisticStateMachine
    {
        public Text environmentText, temperatureText;
        private TimeListener timeListener = new TimeListener();
        private List<string> _visitedEnvironments = new List<string>();
        
        public void Start()
        {
            VisitAllStates();
            LoadEnvironments();
            LoadProbabilities("EnvironmentProbabilityTable");
            NavigateToState("Oasis");
            timeListener.OnTravel(GenerateEnvironment);
            timeListener.OnMinute(UpdateTemperature);
//            TestEnvironmentGenerator();
        }

        public override void NavigateToState(string stateName)
        {
            base.NavigateToState(stateName);
            _visitedEnvironments.Add(stateName);
            RegionManager.GenerateNewRegions();
        }

        private void TestEnvironmentGenerator()
        {
            int expectedTransitions = StatesAsList().Count;
            int totalTransitions;
            int fails = 0;
            int trials = 10000;
            for (int i = 0; i < trials; ++i)
            {
                _visitedEnvironments.Clear();
                NavigateToState("Oasis");
                totalTransitions = 1;
                try
                {
                    while (true)
                    {
                        GenerateEnvironment();
                        ++totalTransitions;
                    }
                }
                catch(KeyNotFoundException e)
                {
                    if (totalTransitions != expectedTransitions)
                    {
                        Helper.PrintList(_visitedEnvironments);
                        ++fails;
                    }
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
            temperatureText.text = ((Environment)GetCurrentState()).GetTemperature(WorldTime.Hours, WorldTime.Minutes) + "\u00B0" + "C";
        }

        private void GenerateEnvironment()
        {
            ((Environment)GetCurrentState()).NextEnvironment(_visitedEnvironments);
            environmentText.text = ((Environment)GetCurrentState()).GetDisplayName();
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