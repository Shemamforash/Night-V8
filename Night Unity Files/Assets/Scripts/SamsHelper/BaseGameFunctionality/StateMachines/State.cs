using System;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class State
    {
        private readonly string _name;
        protected StateMachine ParentMachine;
        public event Action OnUpdate;
        private event Action OnExit;

        protected State(string name, StateMachine parentMachine)
        {
            _name = name;
            ParentMachine = parentMachine;
        }

        public string Name()
        {
            return _name;
        }

        public void AddOnExit(Action exitCallback)
        {
            OnExit += exitCallback;
        }

        public void ClearOnExit()
        {
            OnExit = null;
        }

        public void Update()
        {
            if (OnUpdate != null) OnUpdate();
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
            if (OnExit != null) OnExit();
        }

        public virtual void OnInputDown(InputAxis inputAxis)
        {
        }

        public virtual void OnInputUp(InputAxis inputAxis)
        {
        }
    }
}