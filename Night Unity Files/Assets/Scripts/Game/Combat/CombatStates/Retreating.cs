using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Retreating : CombatState {
        public Retreating(CombatStateMachine parentMachine) : base("Retreating", parentMachine)
        {
            OnUpdate += () => CombatMachine.Character.IncreaseDistance();
        }

        public override void Enter()
        {
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Vertical)
            {
                ParentMachine.NavigateToState("Aiming");
            }
        }
    }
}
