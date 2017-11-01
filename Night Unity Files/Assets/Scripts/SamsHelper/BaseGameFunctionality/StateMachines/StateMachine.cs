using System.Collections.Generic;
using System.Linq;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class StateMachine<T>: IInputListener where T : State
    {
        protected readonly Dictionary<string, T> States = new Dictionary<string, T>();
        private T _currentState;
        private string _defaultState;

        public void EnableInput()
        {
            InputHandler.RegisterInputListener(this);
        }

        public List<T> StatesAsList()
        {
            return new List<T>(States.Values);
        }

        public T GetCurrentState()
        {
            return _currentState;
        }

        public void SetDefaultState(string defaultState)
        {
            _defaultState = defaultState;
        }

        public T GetState(string stateName)
        {
            return States[stateName];
        }

        public void AddState(T state)
        {
            States[state.Name] = state;
        }

        public virtual T NavigateToState(string stateName)
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

        public bool IsInState(T state)
        {
            return _currentState == state;
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
            if (_defaultState == null)
            {
                throw new Exceptions.DefaultStateNotSpecifiedException();
            }
            if (_currentState.Name != _defaultState)
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