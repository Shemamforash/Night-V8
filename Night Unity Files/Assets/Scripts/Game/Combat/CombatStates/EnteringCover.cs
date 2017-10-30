using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class EnteringCover : CombatState {
        public EnteringCover(CombatStateMachine parentMachine) : base("Entering Cover", parentMachine)
        { 
            OnUpdate += () => CombatManager.TakeCover(Character());
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.CancelCover)
            {
                ParentMachine.NavigateToState("Aiming");
            }
        }
    }
}
