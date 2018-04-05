using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class State : MyGameObject, IInputListener
    {
        private event Action OnExit;
        private readonly StateMachine _stateMachine;

        protected State(StateMachine stateMachine, string name) : base(name, GameObjectType.State)
        {
            _stateMachine = stateMachine;
            _stateMachine.AddState(this);
        }

        public void AddOnExit(Action exitCallback)
        {
            OnExit += exitCallback;
        }
        
        public virtual void Enter()
        {
            _stateMachine.GetCurrentState()?.Exit();
            _stateMachine.SetCurrentState(this);
        }

        public bool IsCurrentState()
        {
            return _stateMachine.GetCurrentState() == this;
        }

        public virtual void Exit()
        {
            OnExit?.Invoke();
        }

        public virtual void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
        }

        public virtual void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }
    }
}