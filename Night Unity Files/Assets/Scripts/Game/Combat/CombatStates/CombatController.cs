using System;
using Facilitating.Audio;
using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class CombatController : IInputListener
    {
        private Character _character;
        private bool _sprinting;
        private Cooldown _cockingCooldown, _reloadingCooldown, _dashCooldown, _knockdownCooldown;
        private const float KnockdownDuration = 3f, DashDuration = 2f;
        private long _timeAtLastFire;
        private bool _isPlayer;

        private enum CoverLevel
        {
            None,
            Partial,
            Total
        };

        private CoverLevel _coverLevel;

        public CombatController(Character character)
        {
            _character = character;
            SetReloadCooldown();
            SetKnockdownCooldown();
            SetCockCooldown();
            SetDashCooldown();
            if (!(_character is Player)) return;
            _isPlayer = true;
            InputHandler.RegisterInputListener(this);
        }

        //Cooldowns

        private void SetDashCooldown()
        {
            _dashCooldown = CombatManager.CombatCooldowns.CreateCooldown(DashDuration);
            if (!_isPlayer) return;
            _dashCooldown.SetDuringAction(f => CombatManager.CombatUi.DashCooldownController.UpdateCooldownFill(1 - f / DashDuration));
            _dashCooldown.SetEndAction(() => CombatManager.CombatUi.DashCooldownController.Reset());
        }

        private void SetCockCooldown()
        {
            _cockingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            if (!_isPlayer) return;
            _cockingCooldown.SetEndAction(UpdateMagazineUi);
            _cockingCooldown.SetDuringAction(f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        private void SetKnockdownCooldown()
        {
            _knockdownCooldown = CombatManager.CombatCooldowns.CreateCooldown(KnockdownDuration);
            if (!_isPlayer) return;
            _knockdownCooldown.SetEndAction(() => { CombatManager.CombatUi.ConditionsText.text = ""; });
            _knockdownCooldown.SetDuringAction(f => CombatManager.CombatUi.ConditionsText.text = "Knocked down! " + Helper.Round(f, 1) + "s");
        }

        private void SetReloadCooldown()
        {
            _reloadingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _reloadingCooldown.SetEndAction(() =>
            {
                _character.Weapon().Cocked = true;
                _character.Weapon().Reload(_character.Inventory());
                if (!_isPlayer) return;
                UpdateMagazineUi();
            });
            if (!_isPlayer) return;
            _reloadingCooldown.SetDuringAction(f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        //MOVEMENT

        private float GetSpeedModifier()
        {
            return (1f + _character.BaseAttributes.Endurance.GetCalculatedValue() / 100f) * Time.deltaTime;
        }

        public void Approach()
        {
            if (Immobilised()) return;
            LeaveCover();
            CombatManager.DecreaseDistance(_character, GetSpeedModifier());
        }

        public void Retreat()
        {
            if (Immobilised()) return;
            LeaveCover();
            CombatManager.IncreaseDistance(_character, GetSpeedModifier());
        }

        private void Move(float direction)
        {
            if (direction > 0)
            {
                Approach();
            }
            else
            {
                Retreat();
            }
        }

        //SPRINTING

        public void StartSprinting()
        {
            if (_sprinting) return;
            _character.BaseAttributes.Endurance.AddModifier(2);
            _sprinting = true;
        }

        public void StopSprinting()
        {
            if (!_sprinting) return;
            _character.BaseAttributes.Endurance.RemoveModifier(2);
            _sprinting = false;
        }

//        public void SetFlanked()
//        {
//            _coverLevel = CoverLevel.Partial;
//        }

        //COVER
        public bool InCover()
        {
            return _coverLevel == CoverLevel.Total;
        }

        public void TakeCover()
        {
            if (Immobilised()) return;
            _coverLevel = CoverLevel.Total;
            CombatManager.TakeCover(_character);
        }

        public void LeaveCover()
        {
            if (Immobilised()) return;
            if (!InCover()) return;
            _coverLevel = CoverLevel.None;
            CombatManager.LeaveCover(_character);
        }

        public bool InPartialCover()
        {
            return _coverLevel == CoverLevel.Partial;
        }

        //COCKING
        public void CockWeapon()
        {
            if (Immobilised()) return;
            if (!_cockingCooldown.Finished()) return;
            float cockTime = _character.Weapon().WeaponAttributes.FireRate.GetCalculatedValue();
            GunFire.Cock(cockTime);
            _cockingCooldown.SetDuration(cockTime);
            _cockingCooldown.Start();
        }

        private void StopCocking()
        {
            if (_cockingCooldown == null || _cockingCooldown.Finished()) return;
            _cockingCooldown.Cancel();
        }

        //RELOADING
        private void TryReload()
        {
            if (Immobilised()) return;
            if (!_character.Weapon().Automatic && !_character.Weapon().Cocked)
            {
                CockWeapon();
            }
            else
            {
                ReloadWeapon();
            }
        }

        public void ReloadWeapon()
        {
            if (Immobilised()) return;
            if (_reloadingCooldown.Running()) return;
            if (_character.Weapon().FullyLoaded()) return;
            if (_character.Inventory().GetResourceQuantity(InventoryResourceType.Ammo) == 0) return;
            float reloadSpeed = _character.Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            CombatManager.CombatUi.EmptyMagazine();
            _reloadingCooldown.SetDuration(reloadSpeed);
            _reloadingCooldown.Start();
        }

        private void StopReloading()
        {
            if (_reloadingCooldown == null || _reloadingCooldown.Finished()) return;
            _reloadingCooldown.Cancel();
        }

        //FIRING

        public void FireWeapon()
        {
            if (Immobilised()) return;
            if (!_character.Weapon().Cocked) return;
            if (_character.Weapon().Empty()) return;
            if (!_character.Weapon().Cocked) return;
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            float targetTime = 1f / _character.Weapon().GetAttributeValue(AttributeType.FireRate) * 1000;
            if (timeElapsed < targetTime) return;
            CombatManager.FireWeapon(_character);
            if (_character.Weapon().Automatic)
            {
                _timeAtLastFire = Helper.TimeInMillis();
            }
            else
            {
                _character.Weapon().Cocked = false;
            }
            if (_isPlayer) UpdateMagazineUi();
        }

        //DASHING

        private void Dash(float direction)
        {
            if (direction < 0)
            {
                DashBackward();
                return;
            }
            DashForward();
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished();
        }

        private void DashForward()
        {
            if (Immobilised()) return;
            if (!CanDash()) return;
            CombatManager.DashForward(_character);
            _dashCooldown.Start();
        }

        private void DashBackward()
        {
            if (Immobilised()) return;
            if (!CanDash()) return;
            CombatManager.DashBackward(_character);
            _dashCooldown.Start();
        }

        //MISC

        public void Interrupt()
        {
            StopCocking();
            StopReloading();
            StopSprinting();
            UpdateMagazineUi();
        }

        private void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (!_character.Weapon().Cocked) magazineMessage = "EJECT CARTRIDGE";
            else if (_character.Inventory().GetResourceQuantity(InventoryResourceType.Ammo) == 0) magazineMessage = "NO AMMO";
            else if (_character.Weapon().Empty()) magazineMessage = "RELOAD";

            if (magazineMessage == "")
            {
                CombatManager.CombatUi.UpdateMagazine(_character.Weapon().GetRemainingAmmo());
            }
            else
            {
                CombatManager.CombatUi.EmptyMagazine();
                CombatManager.CombatUi.SetMagazineText(magazineMessage);
            }
        }

        private void Flank()
        {
            if (Immobilised()) return;
            CombatManager.Flank(_character);
        }

        private bool Immobilised()
        {
            return _reloadingCooldown.Running() || _cockingCooldown.Running() || _knockdownCooldown.Running();
        }

        public void KnockDown()
        {
            if (_knockdownCooldown.Running()) return;
            _knockdownCooldown.Start();
        }

        //INPUT

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (Immobilised()) return;
            switch (axis)
            {
                case InputAxis.CancelCover:
                    TakeCover();
                    break;
                case InputAxis.Submit:
                    CombatManager.TryStartRageMode();
                    break;
                case InputAxis.Fire:
                    FireWeapon();
                    break;
                case InputAxis.Flank:
                    Flank();
                    break;
                case InputAxis.Reload:
                    TryReload();
                    break;
                case InputAxis.Vertical:
                    break;
                case InputAxis.Horizontal:
                    Move(direction);
                    break;
                case InputAxis.Sprint:
                    StartSprinting();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.CancelCover:
                    break;
                case InputAxis.Submit:
                    break;
                case InputAxis.Fire:
                    break;
                case InputAxis.Flank:
                    break;
                case InputAxis.Reload:
                    break;
                case InputAxis.Vertical:
                    break;
                case InputAxis.Horizontal:
                    break;
                case InputAxis.Sprint:
                    StopSprinting();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
            if (axis == InputAxis.Horizontal)
            {
                Dash(direction);
            }
        }
    }
}