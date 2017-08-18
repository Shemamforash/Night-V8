using System;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality
{
    public abstract class State
    {
        private string _name;
        protected StateMachine ParentMachine;
        protected Action UpdateCallback;

        public State(string name, StateMachine parentMachine)
        {
            _name = name;
            ParentMachine = parentMachine;
        }

        public State(string name, StateMachine parentMachine, Action updateCallback) : this(name, parentMachine)
        {
            UpdateCallback = updateCallback;
        }

        public string Name()
        {
            return _name;
        }

        public virtual void Update()
        {
            if (UpdateCallback != null)
            {
                UpdateCallback();
            }
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void OnInputDown(InputAxis inputAxis)
        {
        }

        public virtual void OnInputUp(InputAxis inputAxis)
        {
        }
    }
}