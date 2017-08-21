using System;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality
{
    public abstract class State
    {
        private readonly string _name;
        protected StateMachine ParentMachine;
        private Action _updateCallback;

        protected State(string name, StateMachine parentMachine)
        {
            _name = name;
            ParentMachine = parentMachine;
        }

        public string Name()
        {
            return _name;
        }

        public void SetUpdateCallback(Action updateCallback)
        {
            _updateCallback = updateCallback;
        }
        
        public void TryUpdateCallback()
        {
            if (_updateCallback != null)
            {
                _updateCallback();
            }
        }

        public virtual void Update()
        {
            if (_updateCallback != null)
            {
                _updateCallback();
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