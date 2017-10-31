using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Aiming : CombatState
    {
        public Aiming(CombatStateMachine parentMachine) : base("Aiming", parentMachine)
        {
            OnUpdate += IncreaseAim;
        }

        public override void Enter()
        {
            base.Enter();
            if (!Weapon().Cocked) ParentMachine.NavigateToState("Cocking");
            if (Weapon().GetRemainingAmmo() != 0) return;
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("NO AMMO");
        }

        private void IncreaseAim()
        {
            float amount = 5f + ((Weapon)Character().GetGearItem(GearSubtype.Weapon)).GetAttributeValue(AttributeType.Handling) / 10f;
            amount *= Time.deltaTime;
            CombatMachine.AimAmount.Increment(amount);
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            base.OnInputDown(axis, isHeld, direction);
            switch (axis)
            {
                case InputAxis.CancelCover:
                    ParentMachine.NavigateToState("Entering Cover");
                    break;
                case InputAxis.Fire:
                    ParentMachine.NavigateToState("Firing");
                    break;
                case InputAxis.Reload:
                    ParentMachine.NavigateToState("Reloading");
                    break;
                case InputAxis.Horizontal:
                    ParentMachine.NavigateToState(direction > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.Flank:
                    ParentMachine.NavigateToState("Flanking");
                    break;
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}