using System.Collections.Generic;
using System.Linq;
using SamsHelper.Input;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class StateMachine : MonoBehaviour
    {
        protected readonly Dictionary<string, State> States = new Dictionary<string, State>();
        private State _currentState;
        private string _defaultState;
        private readonly InputListener _inputListener = new InputListener();

        public virtual void Awake()
        {
            _inputListener.OnAnyPress(axis => _currentState.OnInputDown(axis));
            _inputListener.OnAnyRelease(axis => _currentState.OnInputUp(axis));
        }

        public virtual List<State> StatesAsList()
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
            _currentState = States[_defaultState];
        }

        public State GetState(string stateName)
        {
            return States[stateName];
        }

        public void AddState(State state)
        {
            States[state.Name()] = state;
        }

        public virtual State NavigateToState(string stateName)
        {
            _currentState = States[stateName];
            _currentState.Enter();
            return _currentState;
        }

        public bool IsInState(string statename)
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