using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class ProbabalisticState : State
    {
        private readonly Dictionary<string, float> _transitionProbabilities = new Dictionary<string, float>();
        private readonly ProbabalisticStateMachine _parentProbabalisticMachine;

        protected ProbabalisticState(string name, StateSubtype subType, ProbabalisticStateMachine parentMachine) : base(name, subType, parentMachine)
        {
            _parentProbabalisticMachine = parentMachine;
        }

        public void AddProbability(string stateName, float probability)
        {
            _transitionProbabilities[stateName] = probability;
        }

        protected string NextState(string lastState)
        {
            return NextState(new List<string> {lastState});
        }

        protected string NextState(List<string> disallowedStates)
        {
            float cumulativeSum = 0;
            float targetValue = UnityEngine.Random.Range(0f, 1.0f);
            string fallBack = "";
            foreach (string stateName in _transitionProbabilities.Keys)
            {
                cumulativeSum += _transitionProbabilities[stateName];
                bool isAllowed = !disallowedStates.Contains(stateName);
                if (!isAllowed) continue;
                if (cumulativeSum >= targetValue)
                {
                    return stateName;
                }
                if (_parentProbabalisticMachine.AllStates() &&
                    _transitionProbabilities[stateName] == 0)
                {
                    return stateName;
                }
                fallBack = stateName;
            }
            return _parentProbabalisticMachine.StrictSampling() ? "" : fallBack;
        }
    }
}