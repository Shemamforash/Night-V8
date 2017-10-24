using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Flanking : CombatState {
        public Flanking(CombatStateMachine parentMachine, bool isPlayerState) : base("Flanking", parentMachine, isPlayerState)
        {
        }

        public override void Enter()
        {
        }
    }
}
