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
        private bool _cocking, _reloading, _sprinting, _knockedDown;
        private Cooldown _cockingCooldown, _reloadingCooldown;
        private float _knockdownDuration = 3f;
        private long _timeAtLastFire;
        private bool _isPlayer;

        public CombatController(Character character)
        {
            _character = character;
            if (!(_character is Player)) return;
            _isPlayer = true;
            InputHandler.RegisterInputListener(this);
        }

        private float GetSpeedModifier()
        {
            return (1f + _character.BaseAttributes.Endurance.GetCalculatedValue() / 100f) * Time.deltaTime;
        }

        private void Approach()
        {
            if (Immobilised()) return;
            LeaveCover();
            CombatManager.DecreaseDistance(_character, GetSpeedModifier());
        }

        private void Retreat()
        {
            if (Immobilised()) return;
            LeaveCover();
            CombatManager.IncreaseDistance(_character, GetSpeedModifier());
        }

        private void TakeCover()
        {
            CombatManager.TakeCover(_character);
        }

        private void LeaveCover()
        {
            CombatManager.LeaveCover(_character);
        }

        private void CockWeapon()
        {
            if (Immobilised()) return;
            if (_cocking) return;
            float cockTime = _character.Weapon().WeaponAttributes.FireRate.GetCalculatedValue();
            GunFire.Cock(cockTime);
            _cocking = true;
            new Cooldown(CombatManager.CombatCooldowns, cockTime, () =>
            {
                _cocking = false;
                UpdateMagazineUi();
            }, f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        private void StopCocking()
        {
            if (_cockingCooldown == null || _cockingCooldown.IsFinished()) return;
            _cockingCooldown.Cancel();
            _cocking = false;
        }

        private void StopReloading()
        {
            if (_reloadingCooldown == null || _reloadingCooldown.IsFinished()) return;
            _reloadingCooldown.Cancel();
            _reloading = false;
        }

        public void Interrupt()
        {
            StopCocking();
            StopReloading();
            StopSprinting();
            UpdateMagazineUi();
        }

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

        private void ReloadWeapon()
        {
            if (Immobilised()) return;
            if (_reloading) return;
            if (_character.Weapon().FullyLoaded()) return;
            if (_character.Inventory().GetResourceQuantity(InventoryResourceType.Ammo) == 0) return;
            float reloadSpeed = _character.Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            _reloading = true;
            CombatManager.CombatUi.EmptyMagazine();
            new Cooldown(CombatManager.CombatCooldowns, reloadSpeed, () =>
            {
                _reloading = false;
                _character.Weapon().Cocked = true;
                _character.Weapon().Reload(_character.Inventory());
                UpdateMagazineUi();
            }, f =>
            {
                Debug.Log("reloaded");
                CombatManager.CombatUi.UpdateReloadTime(f);
            });
        }

        private void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (!_character.Weapon().Cocked) magazineMessage = "EJECT CARTRIDGE";
            else if (_character.Weapon().Empty()) magazineMessage = "NO AMMO";

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

        private void FireWeapon()
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

        private void Flank()
        {
            if (Immobilised()) return;
            CombatManager.Flank(_character);
        }

        private void Dodge()
        {
        }

        private void DashForward()
        {
        }

        private void DashBackward()
        {
        }

        private bool Immobilised()
        {
            return _reloading || _cocking || _knockedDown;
        }

        public void KnockDown()
        {
            if (_knockedDown) return;
            _knockedDown = true;
            new Cooldown(CombatManager.CombatCooldowns, _knockdownDuration, () =>
                {
                    _knockedDown = false;
                    CombatManager.CombatUi.ConditionsText.text = "";
                },
                f => CombatManager.CombatUi.ConditionsText.text = "Knocked down! " + Helper.Round(f, 1) + "s");
        }

        private void Dash(float direction)
        {
            if (Immobilised()) return;
            if (direction < 0)
            {
                DashBackward();
                return;
            }
            DashForward();
        }

        private void StartSprinting()
        {
            if (_sprinting) return;
            _character.BaseAttributes.Endurance.AddModifier(2);
            _sprinting = true;
        }

        private void StopSprinting()
        {
            if (!_sprinting) return;
            _character.BaseAttributes.Endurance.RemoveModifier(2);
            _sprinting = false;
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
    }
}