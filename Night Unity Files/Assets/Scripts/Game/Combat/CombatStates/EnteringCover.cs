namespace Game.Combat.CombatStates
{
    public class EnteringCover : CombatState {
        public EnteringCover(CombatManager parentMachine, bool isPlayerState) : base("EnteringCover", parentMachine, isPlayerState)
        {
        }

        public override void Enter()
        {
            throw new System.NotImplementedException();
        }

        public override void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}
