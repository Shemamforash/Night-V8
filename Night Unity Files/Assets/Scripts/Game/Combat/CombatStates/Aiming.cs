namespace Game.Combat.CombatStates
{
    public class Aiming : CombatState
    {
        public Aiming(CombatStateMachine parentMachine, bool isPlayerState) : base("Aiming", parentMachine, isPlayerState)
        {
            OnUpdate += IncreaseAim;
        }

        public override void Enter()
        {
            if (Weapon().GetRemainingAmmo() != 0) return;
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("NO AMMO");
        }
        
        private void IncreaseAim()
        {
            CombatMachine.IncreaseAim();
        }
    }
}