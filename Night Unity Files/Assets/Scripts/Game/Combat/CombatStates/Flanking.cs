using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Flanking : CombatState
    {
        public Flanking(CombatStateMachine parentMachine) : base("Flanking", parentMachine)
        {
        }

        public override void Update()
        {
            CombatManager.Flank(CombatMachine.Character);
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Flank)
            {
                NavigateToState(nameof(Waiting));
            }
        }
    }
}