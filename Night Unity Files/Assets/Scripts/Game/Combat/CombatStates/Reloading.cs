using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;

namespace Game.Combat.CombatStates
{
    public class Reloading : CombatState {
        public Reloading(CombatStateMachine parentMachine, bool isPlayerState) : base("Reloading", parentMachine, isPlayerState)
        {
        }

        public override void Enter()
        {
            CombatManager.CombatUi.EmptyMagazine();
            new Cooldown(Weapon().GetAttributeValue(AttributeType.ReloadSpeed), Exit, f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        public override void Exit()
        {
            Weapon().Reload();
            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
            ParentMachine.ReturnToDefault();
        }
    }
}
