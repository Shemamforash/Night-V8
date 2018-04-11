using System;
using System.Collections;
using System.Collections.Generic;
using Facilitating;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Player
{
    public class PlayerCombat : CharacterCombat, IInputListener
    {
        private EnemyBehaviour _currentTarget;
        private float _damageModifier, _skillCooldownModifier;
        private Cooldown _dashCooldown;
        private bool _fired;
        private int _initialArmour;
        private Transform _pivot;

        private Coroutine _reloadingCoroutine;

        private Number _strengthText;

        public Characters.Player Player;

        public RageController RageController;
        public bool Retaliate;

        //input
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (IsImmobilised) return;
            if (isHeld)
                switch (axis)
                {
                    case InputAxis.Fire:
                        if (!_fired || Player.Weapon.WeaponAttributes.Automatic) FireWeapon();
                        break;
                    case InputAxis.Horizontal:
                        Move(direction * Vector2.right);
                        break;
                    case InputAxis.Vertical:
                        Move(direction * Vector2.up);
                        break;
                }
            else
                switch (axis)
                {
                    case InputAxis.Enrage:
                        RageController.TryStart();
                        break;
                    case InputAxis.Reload:
                        Reload();
                        break;
                    case InputAxis.SkillOne:
                        SkillBar.ActivateSkill(0);
                        break;
                    case InputAxis.SkillTwo:
                        SkillBar.ActivateSkill(1);
                        break;
                    case InputAxis.SkillThree:
                        SkillBar.ActivateSkill(2);
                        break;
                    case InputAxis.SkillFour:
                        SkillBar.ActivateSkill(3);
                        break;
                    case InputAxis.SwitchTab:
                        UIEnemyController.Select(direction);
                        break;
                    case InputAxis.Sprint:
                        StartSprinting();
                        break;
                }
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.Fire:
                    _fired = false;
                    break;
                case InputAxis.Sprint:
                    StopSprinting();
                    break;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
            switch (axis)
            {
                case InputAxis.Horizontal:
                    Dash(direction * Vector2.right);
                    break;
                case InputAxis.Vertical:
                    Dash(direction * Vector2.up);
                    break;
            }
        }

        public event Action<Shot> OnFireAction;
        public event Action OnReloadAction;

        public override void Kill()
        {
            base.Kill();
            Player.Kill();
            CombatManager.FailCombat();
        }

        public override void Update()
        {
            base.Update();
            if (GetTarget() == null)
            {
                _pivot.gameObject.SetActive(false);
            }
            else
            {
                _pivot.gameObject.SetActive(true);
                _pivot.rotation = AdvancedMaths.RotationToTarget(transform.position, GetTarget().transform.position);
            }
        }

        public void Initialise()
        {

            _pivot = Helper.FindChildWithName<Transform>(gameObject, "Pivot");
            InputHandler.SetCurrentListener(this);

            Player = CharacterManager.SelectedCharacter;
            ArmourController = Player.ArmourController;
            _damageModifier = Player.CalculateDamageModifier();
            _skillCooldownModifier = Player.CalculateSkillCooldownModifier();
            _initialArmour = Player.ArmourController.GetProtectionLevel();

//            _playerUi._playerName.text = player.Name;
            Speed = Player.CalculateSpeed();

            _dashCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _dashCooldown.Duration = Player.CalculateDashCooldown();
            _dashCooldown.SetDuringAction(a =>
            {
                float normalisedTime = a / _dashCooldown.Duration;
                RageBarController.UpdateDashTimer(1 - normalisedTime);
            });
            _dashCooldown.SetEndAction(() =>
            {
                RageBarController.UpdateDashTimer(1);
                RageBarController.PlayFlash();
            });

            RageController = new RageController();
            RageController.EnterCombat();

            HealthController.SetInitialHealth(Player.CalculateCombatHealth(), this);
            HealthController.AddOnHeal(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));

            HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue());

            SkillBar.BindSkills(Player);
            UIMagazineController.SetWeapon(Weapon());
        }

        protected override void Dash(Vector2 direction)
        {
            if (!CanDash()) return;
            base.Dash(direction);
            _dashCooldown.Start();
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished();
        }

        public void TryRetaliate(EnemyBehaviour origin)
        {
            if (Retaliate) FireWeapon();
        }

        public override void Knockback(Vector3 source, float force = 10f)
        {
            base.Knockback(source, force);
            StopReloading();
            StopSprinting();
            UpdateMagazineUi();
        }

        public void SetTarget(EnemyBehaviour e)
        {
            _currentTarget = e;
        }

        public override Weapon Weapon()
        {
            return Player.Weapon;
        }

        public override void ExitCombat()
        {
            StopReloading();
            _fired = false;
        }

        //RELOADING
        private void Reload()
        {
            if (_reloadingCoroutine != null) return;
            if (Player.Weapon.FullyLoaded()) return;
            if (Player.Weapon.GetRemainingMagazines() == 0) return;
            _reloadingCoroutine = StartCoroutine(StartReloading());
        }

        private void StopReloading()
        {
            if (_reloadingCoroutine != null) StopCoroutine(_reloadingCoroutine);
            _reloadingCoroutine = null;
            Immobilised(false);
            UpdateMagazineUi();
        }

        //COOLDOWNS

        private IEnumerator StartReloading()
        {
            Immobilised(true);
            float duration = Player.Weapon.GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            _fired = false;
            OnFireAction = null;
            Retaliate = false;

            float age = 0;
            while (age < duration)
            {
                age += Time.deltaTime;
                float t = age / duration;
                if (t < 0.2f)
                {
                    UIMagazineController.EmptyMagazine();
                }
                else
                {
                    t = (t - 0.2f) / 0.8f;
                    UIMagazineController.UpdateReloadTime(t);
                }

                yield return null;
            }

            Player.Weapon.Reload(Player.Inventory());
            OnReloadAction?.Invoke();
            StopReloading();
        }

        //FIRING
        public void FireWeapon()
        {
            if (GetTarget() == null) return;
            if (Weapon().Empty()) return;
            List<Shot> shots = Weapon().Fire(this);
            if (shots == null) return;
            shots.ForEach(shot =>
            {
                shot.SetDamageModifier(_damageModifier);
                OnFireAction?.Invoke(shot);
                shot.Fire();
            });
            _fired = true;

            UpdateMagazineUi();
        }

        //MISC

        public void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (Player.Weapon.GetRemainingMagazines() == 0) magazineMessage = "NO AMMO";
            else if (Player.Weapon.Empty())
                magazineMessage = "RELOAD";
            if (magazineMessage == "")
            {
                UIMagazineController.UpdateMagazine();
            }
            else
            {
                UIMagazineController.EmptyMagazine();
                UIMagazineController.SetMessage(magazineMessage);
            }
        }

        public override CharacterCombat GetTarget()
        {
            return _currentTarget;
        }
    }
}