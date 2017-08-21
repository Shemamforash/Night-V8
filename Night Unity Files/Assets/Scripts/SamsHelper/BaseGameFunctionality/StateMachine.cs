using System.Collections.Generic;
using System.Linq;
using SamsHelper.Input;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality
{
    public class StateMachine : MonoBehaviour
    {
        private readonly Dictionary<string, State> _states = new Dictionary<string, State>();
        protected State CurrentState;
        private string _defaultState;
        private InputListener _inputListener = new InputListener();

        public virtual void Awake()
        {
            _inputListener.OnAnyPress(axis => CurrentState.OnInputDown(axis));
            _inputListener.OnAnyRelease(axis => CurrentState.OnInputUp(axis));
        }

        public List<State> States()
        {
            return new List<State>(_states.Values);
        }
        
        public void SetDefaultState(string defaultState)
        {
            _defaultState = defaultState;
            CurrentState = _states[_defaultState];
        }

        public State GetState(string stateName)
        {
            return _states[stateName];
        }

        public void AddState(State state)
        {
            _states[state.Name()] = state;
        }

        public void NavigateToState(string stateName)
        {
            CurrentState = _states[stateName];
            CurrentState.Enter();
        }

        public bool IsInState(string statename)
        {
            return CurrentState.Name() == statename;
        }

        public bool IsInState(string[] stateNames)
        {
            return stateNames.Any(IsInState);
        }

        public void Update()
        {
            CurrentState.Update();
        }

        public void ReturnToDefault()
        {
            if (_defaultState != null && CurrentState.Name() != _defaultState)
            {
                NavigateToState(_defaultState);
            }
        }
    }
}