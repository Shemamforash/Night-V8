using System.Collections.Generic;
using Game.World.Time;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World.Environment
{
    public class EnvironmentManager : ProbabalisticStateMachine
    {
        private Text _environmentText, _temperatureText;
        private List<string> _visitedEnvironments = new List<string>();

        public void Awake()
        {
            _environmentText = GameObject.Find("Environment").GetComponent<Text>();
            _temperatureText = GameObject.Find("Temperature").GetComponent<Text>();
        }
        
        public void Start()
        {
            VisitAllStates();
            LoadEnvironments();
            LoadProbabilities("EnvironmentProbabilityTable");
            NavigateToState("Oasis");
            WorldTime.Instance().TravelEvent += GenerateEnvironment;
            WorldTime.Instance().MinuteEvent += UpdateTemperature;
//            TestEnvironmentGenerator();
        }

        public override State NavigateToState(string stateName)
        {
            State newState = base.NavigateToState(stateName);
            _visitedEnvironments.Add(stateName);
            RegionManager.GenerateNewRegions();
            return newState;
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
                global::Game.World.Environment.Environment environment = new global::Game.World.Environment.Environment(
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
            _temperatureText.text = ((global::Game.World.Environment.Environment)GetCurrentState()).GetTemperature(WorldTime.Hours, WorldTime.Minutes) + "\u00B0" + "C";
        }

        private void GenerateEnvironment()
        {
            ((global::Game.World.Environment.Environment)GetCurrentState()).NextEnvironment(_visitedEnvironments);
            _environmentText.text = ((global::Game.World.Environment.Environment)GetCurrentState()).GetDisplayName();
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