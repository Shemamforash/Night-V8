﻿using System;
using System.Collections.Generic;
using Assets;
using Facilitating.Audio;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.CharacterUi;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat
{
    public class PlayerCombat : CharacterCombat, IInputListener
    {
        private float _damageModifier, _skillCooldownModifier;
        private int _initialArmour;
        private Cooldown _dashCooldown;

        public Player Player;

        public RageController RageController;
        private Cooldown _reloadingCooldown;
        public event Action<Shot> OnFireAction;
        public event Action OnReloadAction;
        public bool Retaliate;
        private bool _fired;
        private EnemyBehaviour _currentTarget;

        private Number _strengthText;
        private PlayerUi _playerUi;
        private Transform _pivot;

        public override void Kill()
        {
            base.Kill();
            Player.Kill();
            CombatManager.FailCombat();
        }

        public override void Update()
        {
            if (GetTarget() == null)
            {
                _pivot.gameObject.SetActive(false);
            }
            else
            {
                _pivot.gameObject.SetActive(true);
                _pivot.rotation = AdvancedMaths.RotationToTarget(transform.position, GetTarget().transform.position);
            }

            if (MeleeController.InMelee) return;
            base.Update();
        }

        public void Initialise(Player player)
        {
            _pivot = Helper.FindChildWithName<Transform>(gameObject, "Pivot");
            InputHandler.RegisterInputListener(this);
            
            _playerUi = GameObject.Find("Player Ui").GetComponent<PlayerUi>();
            
            Player = player;
            _damageModifier = Player.CalculateDamageModifier();
            _skillCooldownModifier = Player.CalculateSkillCooldownModifier();
            _initialArmour = player.ArmourController.GetProtectionLevel();
            
            _playerUi._playerName.text = player.Name;
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

            HealthController().SetInitialHealth(Player.CalculateCombatHealth(), this);
            HealthController().AddOnHeal(a => HeartBeatController.SetHealth(HealthController().GetNormalisedHealthValue()));
            HealthController().AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController().GetNormalisedHealthValue()));
            
            HeartBeatController.SetHealth(HealthController().GetNormalisedHealthValue());
            RecoilManager.EnterCombat();

            _playerUi.ArmourController.SetCharacter(Player);
            
            SkillBar.BindSkills(Player);
            InputHandler.RegisterInputListener(this);
            UIMagazineController.SetWeapon(Weapon());
            
            SetReloadCooldown();
            SetDistance(0, 0);
            
            SetConditions();
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

//        protected override void KnockDown()
//        {
//            if (IsKnockedDown) return;
//            base.KnockDown();
//            UIKnockdownController.StartKnockdown(10);
//        }
        
        protected override void KnockDown()
        {
            StopReloading();
//            StopSprinting();
            UpdateMagazineUi();
        }

        public void SetTarget(EnemyBehaviour e)
        {
            _currentTarget = e;
        }

        //MISC
//        public override bool Immobilised()
//        {
//            return _reloadingCooldown.Running() || IsKnockedDown;
//        }


        public override Weapon Weapon()
        {
            return Player.Weapon;
        }

        public override UIHealthBarController HealthController()
        {
            return _playerUi.HealthController;
        }
        
        public override UIArmourController ArmourController()
        {
            return _playerUi.ArmourController;
        }

        public override void ExitCombat()
        {
            UIKnockdownController.Exit();
            MeleeController.Exit();
//            IsKnockedDown = false;
            StopReloading();
            InputHandler.UnregisterInputListener(this);
            _fired = false;
        }

        //RELOADING
        private void Reload()
        {
            if (Immobilised()) return;
            if (_reloadingCooldown.Running()) return;
            if (Player.Weapon.FullyLoaded()) return;
            if (Player.Weapon.GetRemainingMagazines() == 0) return;
            OnFireAction = null;
            Retaliate = false;
            float reloadSpeed = Player.Weapon.GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            _reloadingCooldown.Duration = reloadSpeed;
            _reloadingCooldown.Start();
            _fired = false;
        }

        private void StopReloading()
        {
//            if (_reloadingCooldown == null || _reloadingCooldown.Finished()) return;
//            _reloadingCooldown.Cancel();
        }

        //COOLDOWNS

        private void SetReloadCooldown()
        {
            _reloadingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _reloadingCooldown.SetStartAction(() =>
            {
                Player.Weapon.Reload(Player.Inventory());
                UpdateMagazineUi();
            });
            _reloadingCooldown.SetDuringAction(t =>
            {
                if (t > _reloadingCooldown.Duration * 0.8f)
                {
                    UIMagazineController.EmptyMagazine();
                }
                else
                {
                    t = (t - _reloadingCooldown.Duration * 0.2f) / (_reloadingCooldown.Duration * 0.8f);
                    t = 1 - t;
                    UIMagazineController.UpdateReloadTime(t);
                }
            });
            _reloadingCooldown.SetEndAction(() => OnReloadAction?.Invoke());
        }

        //FIRING
        public void FireWeapon()
        {
            if (Weapon().Empty()) return;
            List<Shot> shots = Weapon().Fire(this);
            if (shots == null) return;
            shots.ForEach(shot =>
            {
                if (RageController.Active()) shot.GuaranteeCritical();
                shot.SetDamageModifier(_damageModifier);
                OnFireAction?.Invoke(shot);
                shot.Fire();
                _fired = true;
            });
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

        private void TryMelee()
        {
//            if (CurrentTarget.DistanceToPlayer > MeleeDistance) return;
//            MeleeController.StartMelee(CurrentTarget);
        }


        //input
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (Immobilised()) return;
            if (isHeld)
            {
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
            }
            else
            {
                switch (axis)
                {
                    case InputAxis.Enrage:
                        RageController.TryStart();
                        break;
                    case InputAxis.Reload:
                        Reload();
                        break;
                    case InputAxis.Melee:
                        TryMelee();
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
    }
}