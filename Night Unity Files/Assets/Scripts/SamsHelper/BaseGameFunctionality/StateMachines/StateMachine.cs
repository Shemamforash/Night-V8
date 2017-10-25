using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using SamsHelper.Input;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class StateMachine: IInputListener
    {
        protected readonly Dictionary<string, State> States = new Dictionary<string, State>();
        private State _currentState;
        private string _defaultState;

        public void EnableInput()
        {
            InputHandler.RegisterInputListener(this);
        }
        
        public List<State> StatesAsList()
        {
            return new List<State>(States.Values);
        }

        public State GetCurrentState()
        {
            return _currentState;
        }

        public void SetDefaultState(string defaultState)
        {
            _defaultState = defaultState;
            NavigateToState(defaultState);
        }

        public State GetState(string stateName)
        {
            return States[stateName];
        }

        public void AddState(State state)
        {
            States[state.Name] = state;
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

        public bool IsInState(string statename)
        {
            return _currentState?.Name == statename;
        }

        public bool IsInState(string[] stateNames)
        {
            return stateNames.Any(IsInState);
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void ReturnToDefault()
        {
            if (_defaultState != null && _currentState.Name != _defaultState)
            {
                NavigateToState(_defaultState);
            }
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            _currentState?.OnInputDown(axis, isHeld, direction);
        }

        public void OnInputUp(InputAxis axis)
        {
            _currentState?.OnInputUp(axis);
        }
    }
}