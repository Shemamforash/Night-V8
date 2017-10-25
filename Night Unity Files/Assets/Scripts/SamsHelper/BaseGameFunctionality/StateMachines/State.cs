using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class State : MyGameObject, IInputListener
    {
        protected readonly StateMachine ParentMachine;
        public event Action OnUpdate;
        private event Action OnExit;
        private readonly StateSubtype _type;

        protected State(string name, StateSubtype type, StateMachine parentMachine) : base(name, GameObjectType.State)
        {
            _type = type;
            ParentMachine = parentMachine;
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

        public void Update()
        {
            OnUpdate?.Invoke();
        }

        public virtual void Enter()
        {
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
    }
}