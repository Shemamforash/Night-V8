using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class ProbabalisticStateMachine<T> : StateMachine<T> where T : ProbabalisticState
    {
        //Fallback states are not permitted using strict sampling
        private bool _useStrictSampling;

        //Fallback states are not permitted, but all must be visited, therefore unreachable, unvisited states are permitted.
        private bool _useAllStates;

        //States may only be visited once
        private bool _onlyVisitOnce;

        private readonly List<string> _visitedStates = new List<string>();

        public void UseStrictSampling()
        {
            _useStrictSampling = true;
        }

        public void ClearVisitedStates()
        {
            _visitedStates.Clear();
        }
        
        
        public void OnlyVisitOnce()
        {
            _onlyVisitOnce = true;
        }
        
        public void VisitAllStates()
        {
            _useAllStates = true;
        }

        public bool StrictSampling()
        {
            return _useStrictSampling;
        }

        public bool AllStates()
        {
            return _useAllStates;
        }

        public override T NavigateToState(string previousState)
        {
            string nextState = CalculateNextState();
            if(_onlyVisitOnce) _visitedStates.Add(previousState);
            return base.NavigateToState(nextState);
        }

        private string CalculateNextState()
        {
            float cumulativeSum = 0;
            float targetValue = Random.Range(0f, 1.0f);
            string fallBack = "";
            Dictionary<string, float> transitionProbabilities = GetCurrentState()?.TransitionProbabilities;
            if (transitionProbabilities != null)
            {
                foreach (string stateName in transitionProbabilities.Keys)
                {
                    cumulativeSum += transitionProbabilities[stateName];
                    if (_onlyVisitOnce && _visitedStates.Contains(stateName)) continue;
                    if (cumulativeSum >= targetValue)
                    {
                        return stateName;
                    }
                    if (AllStates() &&
                        transitionProbabilities[stateName] == 0)
                    {
                        return stateName;
                    }
                    fallBack = stateName;
                }
            }
            return StrictSampling() ? "" : fallBack;
        }

        protected void LoadProbabilities(string fileName)
        {
            List<string> lines = Helper.ReadLinesFromFile(fileName);
            string[] headings = lines[0].Split(',');
            for (int i = 1; i < lines.Count; ++i)
            {
                string[] probabilities = lines[i].Split(',');
                ProbabalisticState state = States[probabilities[0]];
                for (int j = 1; j < probabilities.Length; ++j)
                {
                    state.AddProbability(headings[j], float.Parse(probabilities[j]));
                }
            }
        }
    }
}