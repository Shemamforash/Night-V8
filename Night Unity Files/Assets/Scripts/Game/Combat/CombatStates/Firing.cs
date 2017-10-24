using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Firing : CombatState
    {
        private float _timeSinceLastFire;

        public Firing(CombatStateMachine parentMachine, bool isPlayerState) : base("Firing", parentMachine, isPlayerState)
        {
            OnUpdate += FireWeapon;
        }

        private void FireWeapon()
        {
            Debug.Log("Update");
            if (Weapon().GetRemainingAmmo() > 0)
            {
                _timeSinceLastFire -= Time.deltaTime;
                if (_timeSinceLastFire > 0) return;
                Weapon().Fire();
                if (IsPlayerState)
                {
                    CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
                }
                CombatMachine.DecreaseAim();
                if (Weapon().Automatic)
                {
                    _timeSinceLastFire = 1f / Weapon().GetAttributeValue(AttributeType.FireRate);
                }
                else
                {
                    ParentMachine.NavigateToState("Cocking");
                }
            }
            else
            {
                CombatManager.CombatUi.EmptyMagazine();
                CombatManager.CombatUi.SetMagazineText("NO AMMO");
            }
        }

        public override void Enter()
        {
            _timeSinceLastFire = 0f;
        }
    }
}