using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Firing : CombatState
    {
        private float _timeSinceLastFire;

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

        private void TryRepeatFire()
        {
            if (Weapon().Automatic)
            {
                _timeSinceLastFire = 1f / Weapon().GetAttributeValue(AttributeType.FireRate);
            }
            else
            {
                Weapon().Cocked = false;
                ParentMachine.NavigateToState("Cocking");
            }
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (axis == InputAxis.Fire)
            {
                FireWeapon();
            }
        }

        private void FireWeapon()
        {
            Debug.Log("firing");
            if (Weapon().GetRemainingAmmo() > 0 && Weapon().Cocked)
            {
                _timeSinceLastFire -= Time.deltaTime;
                if (_timeSinceLastFire > 0) return;
                Weapon().Fire();
                CombatMachine.DecreaseAim();
                UpdateMagazineUi();
                TryRepeatFire();
            }
            else if (Weapon().GetRemainingAmmo() == 0 && Character().Inventory().GetResourceQuantity(InventoryResourceType.Ammo) != 0)
            {
                ParentMachine.NavigateToState("Reloading");
            }
        }

        public override void Enter()
        {
            _timeSinceLastFire = 0f;
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}