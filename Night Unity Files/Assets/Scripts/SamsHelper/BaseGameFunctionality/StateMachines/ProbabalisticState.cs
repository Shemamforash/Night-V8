using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class ProbabalisticState : State
    {
        private readonly Dictionary<string, float> _transitionProbabilities = new Dictionary<string, float>();
        private readonly ProbabalisticStateMachine _parentProbabalisticMachine;

        public ProbabalisticState(string name, ProbabalisticStateMachine parentMachine) : base(name, parentMachine)
        {
            _parentProbabalisticMachine = parentMachine;
        }

        public void AddProbability(string stateName, float probability)
        {
            _transitionProbabilities[stateName] = probability;
        }

        public string NextState()
        {
            return NextState("");
        }

        public string NextState(string lastState)
        {
            return NextState(new List<string> {lastState});
        }

        public string NextState(List<string> disallowedStates)
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
            if (_parentProbabalisticMachine.StrictSampling())
            {
                return "";
            }
            return fallBack;
        }
    }
}