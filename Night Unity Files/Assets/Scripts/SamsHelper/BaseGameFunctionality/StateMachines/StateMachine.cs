using System.Collections.Generic;
using Game.Characters.CharacterActions;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public class StateMachine
    {
        protected readonly Dictionary<string, State> States = new Dictionary<string, State>();
        private State _currentState;
        private State _defaultState;

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
            _defaultState.Enter();
        }

        public State GetState(string stateName)
        {
            State state = States[stateName];
            if (state == null) throw new Exceptions.StateDoesNotExistException(stateName);
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
            if (_defaultState == null) throw new Exceptions.DefaultStateNotSpecifiedException();
            if (_currentState == null || !_defaultState.IsCurrentState()) _defaultState.Enter();
        }

        public bool IsDefaultState(BaseCharacterAction currentState)
        {
            return currentState == _defaultState;
        }
    }
}