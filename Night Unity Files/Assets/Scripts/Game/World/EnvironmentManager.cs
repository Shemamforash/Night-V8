using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using UnityEngine;

namespace World
{
    using Articy.Unity;
    using Articy.Night;
    using UnityEngine.UI;
    using Menus;

    public class EnvironmentManager : ProbabalisticStateMachine
    {
        public ArticyRef articyEnvironments;
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

//        private Environment SelectEnvironmentChoice(Environment disallowed)
//        {
//            int dangerIndex = (int) Mathf.Floor(WorldState.DangerLevel);
//            float[] pDistribution = new float[] {0.15f, 0.2f, 0.3f, 0.2f, 0.15f};
//            float rand = Random.Range(0f, 1f);
//            float currentPVal = 0f;
//            for (int i = 0; i < pDistribution.Length; ++i)
//            {
//                currentPVal += pDistribution[i];
//                int j = dangerIndex - 2 + i;
//                if (j < 0)
//                {
//                    j = 0;
//                }
//                else if (j >= environments.Length)
//                {
//                    j = environments.Length - 1;
//                }
//                if (rand <= currentPVal)
//                {
//                    if (environments[j] == disallowed)
//                    {
//                        if (j - 1 > 0)
//                        {
//                            return environments[j - 1];
//                        }
//                        return environments[j + 1];
//                    }
//                    return environments[j];
//                }
//            }
//            return null;
//        }
    }
}