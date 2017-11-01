using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class EnteringCover : CombatState {
        public EnteringCover(CombatStateMachine parentMachine) : base("Entering Cover", parentMachine)
        { 
        }

        public override void Update()
        {
            CombatManager.TakeCover(Character());
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.CancelCover)
            {
                NavigateToState(nameof(Waiting));
            }
        }
    }
}
