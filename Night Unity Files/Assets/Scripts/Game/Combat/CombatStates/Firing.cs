using System;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
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

        public Firing(CombatStateMachine parentMachine) : base(nameof(Firing), parentMachine)
        {
        }

//        private void UpdateMagazineUi()
//        {
//            if (Weapon().GetRemainingAmmo() == 0)
//            {
//                CombatManager.CombatUi.EmptyMagazine();
//                CombatManager.CombatUi.SetMagazineText("NO AMMO");
//            }
//            else
//            {
//                CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
//            }
//        }
//
//        private void TryRepeatFire()
//        {
//            if (Weapon().Automatic)
//            {
//                _timeAtLastFire = Helper.TimeInMillis();
//            }
//            else
//            {
//                Weapon().Cocked = false;
//                NavigateToState(nameof(Cocking));
//            }
//        }
//
//        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
//        {
//            base.OnInputDown(axis, isHeld, direction);
//            if (axis == InputAxis.Fire)
//            {
//                FireWeapon();
//            }
//        }
//
//        private void FireWeapon()
//        {
//            if (Weapon().GetRemainingAmmo() > 0 && Weapon().Cocked)
//            {
//                long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
//                float targetTime = 1f / Weapon().GetAttributeValue(AttributeType.FireRate) * 1000;
//                if (timeElapsed < targetTime) return;
//                CombatManager.FireWeapon(Character());
//                if (IsPlayer) UpdateMagazineUi();
//                TryRepeatFire();
//            }
//            else if (Weapon().GetRemainingAmmo() == 0 && Character().Inventory().GetResourceQuantity(InventoryResourceType.Ammo) != 0)
//            {
//                NavigateToState(nameof(Reloading));
//            }
//        }
//
//        public override void OnInputUp(InputAxis axis)
//        {
//            base.OnInputUp(axis);
//            if (axis == InputAxis.Fire) NavigateToState(nameof(Waiting));
//        }
    }
}