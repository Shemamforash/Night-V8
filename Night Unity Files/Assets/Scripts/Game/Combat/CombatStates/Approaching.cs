namespace Game.Combat.CombatStates
{
    public class Approaching : CombatState
    {
        public Approaching(CombatManager parentMachine, bool isPlayerState) : base("Approaching", parentMachine, isPlayerState)
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