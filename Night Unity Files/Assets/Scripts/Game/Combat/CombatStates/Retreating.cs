using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Retreating : CombatState {
        public Retreating(CombatStateMachine parentMachine) : base("Retreating", parentMachine)
        {
            OnUpdate += () => CombatMachine.Character.IncreaseDistance();
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Horizontal)
            {
                ParentMachine.NavigateToState("Aiming");
            }
        }
    }
}
