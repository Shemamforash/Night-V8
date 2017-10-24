namespace Game.Combat.CombatStates
{
    public class Approaching : CombatState
    {
        public Approaching(CombatStateMachine parentMachine, bool isPlayerState) : base("Approaching", parentMachine, isPlayerState)
        {
            OnUpdate += () => CombatMachine.Character.DecreaseDistance();
        }
    }
}