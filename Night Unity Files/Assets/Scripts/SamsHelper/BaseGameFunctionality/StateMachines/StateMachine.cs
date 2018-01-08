using System.Collections.Generic;
using SamsHelper.Input;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class StateMachine: IInputListener
    {
        protected readonly Dictionary<string, State> States = new Dictionary<string, State>();
        private State _currentState;
        private State _defaultState;
        private bool _acceptInput = true;

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

        public void SetDefaultState(State defaultState)
        {
            _defaultState = defaultState;
        }

        public State GetState(string stateName)
        {
            return States[stateName];
        }

        public void AddState(State state)
        {
            States[state.Name] = state;
        }

        public void SetCurrentState(State state)
        {
            _currentState = state;
        }

        public void ReturnToDefault()
        {
            if (_defaultState == null)
            {
                throw new Exceptions.DefaultStateNotSpecifiedException();
            }
            if (_currentState == null || !_defaultState.IsCurrentState())
            {
                _defaultState.Enter();
            }
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (!_acceptInput) return;
            _currentState?.OnInputDown(axis, isHeld, direction);
        }

        public void OnInputUp(InputAxis axis)
        {
            if (!_acceptInput) return;
            _currentState?.OnInputUp(axis);
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
            if (!_acceptInput) return;
            _currentState?.OnDoubleTap(axis, direction);
        }
    }
}