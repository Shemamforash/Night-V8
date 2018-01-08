using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class State : MyGameObject, IInputListener
    {
        private event Action OnExit;
        private readonly StateSubtype _type;
        private readonly StateMachine _stateMachine;

        protected State(StateMachine stateMachine, string name, StateSubtype type) : base(name, GameObjectType.State)
        {
            _stateMachine = stateMachine;
            _stateMachine.AddState(this);
            _type = type;
        }

        public void AddOnExit(Action exitCallback)
        {
            OnExit += exitCallback;
        }
        
        public StateSubtype StateType()
        {
            return _type;
        }

        protected void ClearOnExit()
        {
            OnExit = null;
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