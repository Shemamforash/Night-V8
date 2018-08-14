using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class ProbabalisticState : State
    {
        public readonly Dictionary<string, float> TransitionProbabilities = new Dictionary<string, float>();

        protected ProbabalisticState(StateMachine stateMachine, string name, GameObjectType type) : base(stateMachine, name, type)
        {
        }

        public void AddProbability(string stateName, float probability)
        {
            TransitionProbabilities[stateName] = probability;
        }
    }
}