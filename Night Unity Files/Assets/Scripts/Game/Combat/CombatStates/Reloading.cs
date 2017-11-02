using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Reloading : CombatState
    {
        private bool _interrupted;
        private Cooldown _reloadCooldown;
        
        public Reloading(CombatStateMachine parentMachine) : base(nameof(Reloading), parentMachine)
        {
        }

//        public override void Enter()
//        {
//            _interrupted = false;
//            if (Weapon().GetRemainingAmmo() == Weapon().Capacity)
//            {
//                NavigateToState(nameof(Waiting));
//                return;
//            }
//            _reloadCooldown = new Cooldown(CombatManager.CombatCooldowns, Weapon().GetAttributeValue(AttributeType.ReloadSpeed), Reload, f => CombatManager.CombatUi.UpdateReloadTime(f));
//            if (!IsPlayer) return;
//            CombatManager.CombatUi.EmptyMagazine();
//        }
//
//        private void Reload()
//        {
//            if (!_interrupted)
//            {
//                Weapon().Cocked = true;
//                Weapon().Reload(Character().Inventory());
//            }
//            NavigateToState(nameof(Waiting));
//            if (!IsPlayer) return;
//            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
//        }
//
//        public override void Exit()
//        {
//            if (!_reloadCooldown.IsFinished())
//            {
//                Interrupt();
//            }
//        }
//
//        private void Interrupt()
//        {
//            _interrupted = true;
//            _reloadCooldown.Cancel();
//            Reload();
//        }
    }
}
