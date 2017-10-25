using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class EnteringCover : CombatState {
        public EnteringCover(CombatStateMachine parentMachine) : base("EnteringCover", parentMachine)
        {
        }

        public override void Enter()
        {
        }

        public override void Exit()
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
