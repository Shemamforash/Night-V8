namespace Game.Combat.CombatStates
{
    public class Retreating : CombatState {
        public Retreating(CombatStateMachine parentMachine, bool isPlayerState) : base("Retreating", parentMachine, isPlayerState)
        {
            OnUpdate += () => CombatMachine.Character.IncreaseDistance();
        }

        public override void Enter()
        {
        }
    }
}
