using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class EnteringCover : CombatState {
        public EnteringCover(CombatStateMachine parentMachine) : base("EnteringCover", parentMachine)
        { 
            OnUpdate += () => CombatMachine.Character.TakeCover();
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
