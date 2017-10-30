using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Cocking : CombatState
    {
        private Cooldown _cockingCooldown;

        public Cocking(CombatStateMachine parentMachine) : base("Cocking", parentMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("EJECT CARTRIDGE");
        }

        private void SetCock()
        {
            Weapon().Cocked = true;
            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
            ParentMachine.NavigateToState("Aiming");
            Debug.Log("cocked");
            _cockingCooldown = null;
        }

        private void StartCocking()
        {
            CombatManager.CombatUi.EmptyMagazine();
            float fireRate = Weapon().WeaponAttributes.FireRate.GetCalculatedValue();
            _cockingCooldown = new Cooldown(CombatManager.CombatCooldowns, fireRate, SetCock, f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            switch (axis)
            {
                case InputAxis.Reload:
                    if (!isHeld && _cockingCooldown == null)
                    {
                        StartCocking();
                    }
                    break;
                case InputAxis.Horizontal:
                    _cockingCooldown?.Cancel();
                    ParentMachine.NavigateToState(direction > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.CancelCover:
                    _cockingCooldown?.Cancel();
                    ParentMachine.NavigateToState("Entering Cover");
                    break;
                case InputAxis.Flank:
                    _cockingCooldown?.Cancel();
                    ParentMachine.NavigateToState("Flanking");
                    break;
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}