using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class State : MyGameObject
    {
        protected StateMachine ParentMachine;
        public event Action OnUpdate;
        private event Action OnExit;
        public readonly StateSubtype Type;

        protected State(string name, StateSubtype type, StateMachine parentMachine) : base(name, GameObjectType.State)
        {
            Type = type;
            ParentMachine = parentMachine;
        }

        public void AddOnExit(Action exitCallback)
        {
            OnExit += exitCallback;
        }

        public StateSubtype StateType()
        {
            return Type;
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