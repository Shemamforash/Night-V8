using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Flanking : CombatState {
        public Flanking(CombatStateMachine parentMachine) : base("Flanking", parentMachine)
        {
        }

        public override void Enter()
        {
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}
