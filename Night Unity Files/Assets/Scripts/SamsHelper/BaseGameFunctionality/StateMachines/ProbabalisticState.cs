﻿using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class ProbabalisticState : State
    {
        public readonly Dictionary<string, float> TransitionProbabilities = new Dictionary<string, float>();

        protected ProbabalisticState(StateMachine stateMachine, string name) : base(stateMachine, name)
        {
        }

        public void AddProbability(string stateName, float probability)
        {
            TransitionProbabilities[stateName] = probability;
        }
    }
}