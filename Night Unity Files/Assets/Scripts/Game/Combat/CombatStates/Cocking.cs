using Facilitating.Audio;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Cocking : CombatState
    {
        private Cooldown _cockingCooldown;

        public Cocking(CombatStateMachine parentMachine) : base(nameof(Cocking), parentMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (!IsPlayer) return;
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("EJECT CARTRIDGE");
        }

        private void SetCock()
        {
            Weapon().Cocked = true;
            NavigateToState(nameof(Waiting));
            Debug.Log("cocked");
            _cockingCooldown = null;
            if (!IsPlayer) return;
            CombatManager.CombatUi.UpdateMagazine(Weapon().GetRemainingAmmo());
        }

        private void StartCocking()
        {
            float fireRate = Weapon().WeaponAttributes.FireRate.GetCalculatedValue();
            GunFire.Cock(fireRate);
            _cockingCooldown = new Cooldown(CombatManager.CombatCooldowns, fireRate, SetCock, f => CombatManager.CombatUi.UpdateReloadTime(f));
            if (!IsPlayer) return;
            CombatManager.CombatUi.EmptyMagazine();
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            base.OnInputDown(axis, isHeld, direction);
            switch (axis)
            {
                case InputAxis.Reload:
                    if (Weapon().GetRemainingAmmo() == 0)
                    {
                        NavigateToState(nameof(Reloading));
                    }
                    else if (!isHeld && _cockingCooldown == null)
                    {
                        StartCocking();
                    }
                    break;
                case InputAxis.Horizontal:
                    _cockingCooldown?.Cancel();
                    NavigateToState(direction > 0 ? nameof(Approaching) : nameof(Retreating));
                    break;
                case InputAxis.CancelCover:
                    _cockingCooldown?.Cancel();
                    CombatManager.TakeCover(Character());
                    break;
                case InputAxis.Flank:
                    _cockingCooldown?.Cancel();
                    NavigateToState(nameof(Flanking));
                    break;
            }
        }
    }
}