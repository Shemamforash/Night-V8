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
        
        public Reloading(CombatStateMachine parentMachine) : base("Reloading", parentMachine)
        {
        }

        public override void Enter()
        {
            _interrupted = false;
            if (Weapon().GetRemainingAmmo() == Weapon().Capacity)
            {
                ParentMachine.NavigateToState("Aiming");
                return;
            }
            CombatManager.CombatUi.EmptyMagazine();
            _reloadCooldown = new Cooldown(CombatManager.CombatCooldowns, Weapon().GetAttributeValue(AttributeType.ReloadSpeed), Reload, f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        private void Reload()
        {
            if (!_interrupted)
            {
                Weapon().Cocked = true;
                Weapon().Reload(Character().Inventory());
            }
            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
            ParentMachine.NavigateToState("Aiming");
        }

        private void Interrupt()
        {
            _interrupted = true;
            _reloadCooldown.Cancel();
            Reload();
        }
        
        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            switch (axis)
            {
                case InputAxis.Vertical:
                    Interrupt();
                    ParentMachine.NavigateToState(direction > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.Horizontal:
                    Interrupt();
                    ParentMachine.NavigateToState(direction > 0 ? "Flanking" : "Entering Cover");
                    break;
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}
