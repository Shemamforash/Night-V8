using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class ProbabalisticState : State
    {
        public readonly Dictionary<string, float> TransitionProbabilities = new Dictionary<string, float>();

        protected ProbabalisticState(string name, StateSubtype subType) : base(name, subType)
        {
        }

        public void AddProbability(string stateName, float probability)
        {
            TransitionProbabilities[stateName] = probability;
        }
        
        protected override void ReturnToDefault()
        {
            throw new Exceptions.ProbabalisticMachineHasNoDefaultStateException();
        }
    }
}