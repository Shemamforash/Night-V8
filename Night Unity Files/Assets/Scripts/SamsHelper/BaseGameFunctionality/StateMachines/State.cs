using SamsHelper.Input;

namespace SamsHelper.BaseGameFunctionality.StateMachines
{
    public abstract class State : IInputListener
    {
        private readonly StateMachine _stateMachine;
        public readonly string Name;

        protected State(StateMachine stateMachine, string name)
        {
            Name = name;
            _stateMachine = stateMachine;
            _stateMachine.AddState(this);
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
        }
    }
}