using System;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Firing : CombatState
    {
        private long _timeAtLastFire;

        public Firing(CombatStateMachine parentMachine) : base("Firing", parentMachine)
        {
        }

        private void UpdateMagazineUi()
        {
            if (Weapon().GetRemainingAmmo() == 0)
            {
                CombatManager.CombatUi.EmptyMagazine();
                CombatManager.CombatUi.SetMagazineText("NO AMMO");
            }
            else
            {
                CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
            }
        }

        private long TimeInMillis()
        {
            return DateTime.Now.Ticks / 10000;
        }

        private void TryRepeatFire()
        {
            if (Weapon().Automatic)
            {
                _timeAtLastFire = TimeInMillis();
            }
            else
            {
                Weapon().Cocked = false;
                ParentMachine.NavigateToState("Cocking");
            }
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            base.OnInputDown(axis, isHeld, direction);
            if (axis == InputAxis.Fire)
            {
                FireWeapon();
            }
        }

        private void FireWeapon()
        {
            if (Weapon().GetRemainingAmmo() > 0 && Weapon().Cocked)
            {
                long timeElapsed = TimeInMillis() - _timeAtLastFire;
                float targetTime = 1f / Weapon().GetAttributeValue(AttributeType.FireRate) * 1000;
                if (timeElapsed < targetTime) return;
                CombatManager.FireWeapon(Character());
                DecreaseAim();
                UpdateMagazineUi();
                TryRepeatFire();
            }
            else if (Weapon().GetRemainingAmmo() == 0 && Character().Inventory().GetResourceQuantity(InventoryResourceType.Ammo) != 0)
            {
                ParentMachine.NavigateToState("Reloading");
            }
        }

        private void DecreaseAim()
        {
            float amount = 100f / Character().Weapon().Capacity;
            CombatMachine.AimAmount.Decrement(amount);
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Fire)
            {
                ParentMachine.NavigateToState("Aiming");
            }
        }
    }
}