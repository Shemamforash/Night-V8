using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class StateMachine : MonoBehaviour
    {
        protected readonly Dictionary<string, State> States = new Dictionary<string, State>();
        private State _currentState;
        private string _defaultState;

        protected List<State> StatesAsList()
        {
            return new List<State>(States.Values);
        }

        public State GetCurrentState()
        {
            return _currentState;
        }

        protected void SetDefaultState(string defaultState)
        {
            _defaultState = defaultState;
            NavigateToState(defaultState);
        }

        public State GetState(string stateName)
        {
            return States[stateName];
        }

        protected void AddState(State state)
        {
            States[state.Name()] = state;
        }

        public virtual State NavigateToState(string stateName)
        {
            _currentState?.Exit();
            if (!States.ContainsKey(stateName))
            {
                throw new Exceptions.UnknownStateNameException(stateName);
            }
            _currentState = States[stateName];
            _currentState.Enter();
            return _currentState;
        }

        private bool IsInState(string statename)
        {
            return _currentState.Name() == statename;
        }

        public bool IsInState(string[] stateNames)
        {
            return stateNames.Any(IsInState);
        }

        public void Update()
        {
            _currentState.Update();
        }

        public void ReturnToDefault()
        {
            if (_defaultState != null && _currentState.Name() != _defaultState)
            {
                NavigateToState(_defaultState);
            }
        }
    }
}