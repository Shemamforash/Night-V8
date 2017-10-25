using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Reloading : CombatState
    {
        private bool _interrupted;
        
        public Reloading(CombatStateMachine parentMachine) : base("Reloading", parentMachine)
        {
        }

        public override void Enter()
        {
            if (Weapon().GetRemainingAmmo() == Weapon().Capacity)
            {
                ParentMachine.NavigateToState("Aiming");
                return;
            }
            CombatManager.CombatUi.EmptyMagazine();
            new Cooldown(CombatManager.CombatCooldowns, Weapon().GetAttributeValue(AttributeType.ReloadSpeed), Reload, f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        private void Reload()
        {
            if (_interrupted) return;
            Weapon().Cocked = true;
            Weapon().Reload();
            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
            ParentMachine.NavigateToState("Aiming");
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            switch (axis)
            {
                case InputAxis.Vertical:
                    _interrupted = true;
                    ParentMachine.NavigateToState(direction > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.Horizontal:
                    _interrupted = true;
                    ParentMachine.NavigateToState(direction > 0 ? "Flanking" : "Entering Cover");
                    break;
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}
