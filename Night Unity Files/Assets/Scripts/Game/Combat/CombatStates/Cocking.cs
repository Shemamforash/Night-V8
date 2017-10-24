using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Cocking : CombatState
    {
        public Cocking(CombatStateMachine parentMachine, bool isPlayerState) : base("Cocking", parentMachine, isPlayerState, false)
        {
            InputHandler.Instance().AddOnPressEvent(InputAxis.Cancel, Cock);
        }

        public override void Enter()
        {
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("EJECT CARTRIDGE");
        }

        public override void Exit()
        {
            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
            ParentMachine.ReturnToDefault();
            InputHandler.Instance().RemoveOnPressEvent(InputAxis.Cancel, Cock);
        }

        private void Cock()
        {
            Debug.Log("banana");
            CombatManager.CombatUi.EmptyMagazine();
            new Cooldown(Weapon().WeaponAttributes.FireRate.GetCalculatedValue(), Exit, f => CombatManager.CombatUi.UpdateReloadTime(f));
        }
    }
}