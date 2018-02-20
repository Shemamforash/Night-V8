using System;
using System.Collections.Generic;
using Assets;
using Facilitating.Audio;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Characters.Player;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using TMPro;
using UnityEngine;

namespace Game.Combat
{
    public class PlayerCombat : CharacterCombat, IInputListener
    {
        private float _damageModifier, _skillCooldownModifier;
        private int _initialArmour;

        public Player Player;

        public RageController RageController;
        private Cooldown _reloadingCooldown;
        public event Action<Shot> OnFireAction;
        public event Action OnReloadAction;
        public bool Retaliate;
        private bool _fired;
        public Direction FacingDirection;
        private Cooldown _dashCooldown;
        public DetailedEnemyCombat CurrentTarget;

        private TextMeshProUGUI _playerName;

        public CanvasGroup PlayerCanvasGroup;
        private Number _strengthText;


        public override void Kill()
        {
            Player.Kill();
            CombatManager.FailCombat();
        }

        public override void Update()
        {
            if (MeleeController.InMelee) return;
            base.Update();
            UpdatePlayerDirection();
        }
        
        public override void Awake()
        {
            base.Awake();
            PlayerCanvasGroup = gameObject.GetComponent<CanvasGroup>();
            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            HealthController.AddOnHeal(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            MoveForwardAction = f => Position.Increment(f);
            MoveBackwardAction = f => Position.Decrement(f);
            _dashCooldown = CombatManager.CombatCooldowns.CreateCooldown();
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
            HealthController.SetIsPlayerBar();
            RageController = new RageController();
            SetReloadCooldown();
        }

        public override void SetPlayer(Character player)
        {
            base.SetPlayer(player);
            Player = (Player) player;
            _playerName.text = player.Name;
            Speed = Player.CalculateSpeed();
            Position.SetCurrentValue(0);
            
            _dashCooldown.Duration = Player.CalculateDashCooldown();
            _damageModifier = Player.CalculateDamageModifier();
            _skillCooldownModifier = Player.CalculateSkillCooldownModifier();
            
            HealthController.SetInitialHealth(Player.CalculateCombatHealth(), this);
            _initialArmour = player.ArmourController.GetProtectionLevel();

            HeartBeatController.Enable();
            HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue());
            RecoilManager.EnterCombat();
            
            ArmourController.SetArmourValue(_initialArmour);
            RageController.EnterCombat();
            SkillBar.BindSkills(Player);
            FacingDirection = Direction.Right;
            InputHandler.RegisterInputListener(this);
            UIMagazineController.SetWeapon(Weapon());
        }
        
        public void UpdatePlayerDirection()
        {
            if (CurrentTarget == null) return;
            bool behind = CurrentTarget.Position.CurrentValue() < Position.CurrentValue();
            FacingDirection = behind ? Direction.Left : Direction.Right;
        }

        private void Dash(float direction)
        {
            if (Immobilised() || !CanDash()) return;
            LeaveCover();
            if (direction > 0)
            {
                MoveForwardAction?.Invoke(_dashDistance);
            }
            else
            {
                MoveBackwardAction?.Invoke(_dashDistance);
            }

            _dashCooldown.Start();
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished();
        }

        private float _dashDistance = 5f;

        //SPRINTING

        private void StartSprinting()
        {
            if (Sprinting) return;
            Sprinting = true;
        }

        private void StopSprinting()
        {
            if (!Sprinting) return;
            Sprinting = false;
        }

        private void ChangeCover()
        {
            if (InCover) LeaveCover();
            else TakeCover();
        }

        public void TryRetaliate(DetailedEnemyCombat origin)
        {
            if (Retaliate) FireWeapon(origin);
        }

        protected override void KnockDown()
        {
            if (IsKnockedDown) return;
            base.KnockDown();
            UIKnockdownController.StartKnockdown(10);
        }

        protected override void TakeCover()
        {
            base.TakeCover();
            GunFire.EnterCover();
            PlayerCanvasGroup.alpha = 0.4f;
        }

        protected override void LeaveCover()
        {
            base.LeaveCover();
            GunFire.ExitCover();
            PlayerCanvasGroup.alpha = 1f;
        }

        public void SetTarget(DetailedEnemyCombat e)
        {
            if (e == null) return;
            CurrentTarget = e;
            CurrentTarget.MarkSelected();
        }

        //MISC
        public override bool Immobilised()
        {
            return _reloadingCooldown.Running() || IsKnockedDown;
        }


        public override void ExitCombat()
        {
            UIKnockdownController.Exit();
            MeleeController.Exit();
            IsKnockedDown = false;
            StopReloading();
            InputHandler.UnregisterInputListener(this);
            Position.SetCurrentValue(0);
            _fired = false;
            HeartBeatController.Disable();
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
            if (_reloadingCooldown == null || _reloadingCooldown.Finished()) return;
            _reloadingCooldown.Cancel();
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
        public void FireWeapon(CharacterCombat target)
        {
            List<Shot> shots = Weapon().Fire(target, this);
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

        protected override void Interrupt()
        {
            StopReloading();
            StopSprinting();
            UpdateMagazineUi();
        }

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

        private void TryMelee()
        {
            if (CurrentTarget.DistanceToPlayer > MeleeDistance) return;
            MeleeController.StartMelee(CurrentTarget);
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
                        if (!_fired || Player.Weapon.WeaponAttributes.Automatic) FireWeapon(CurrentTarget);
                        break;
                    case InputAxis.Horizontal:
                        Move(direction);
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case InputAxis.Cover:
                        ChangeCover();
                        break;
                    case InputAxis.Enrage:
                        RageController.TryStart();
                        break;
                    case InputAxis.Reload:
                        Reload();
                        break;
                    case InputAxis.Vertical:
                        UIEnemyController.Select(direction);
                        break;
                    case InputAxis.Sprint:
                        StartSprinting();
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
            if (axis == InputAxis.Horizontal)
            {
                Dash(direction);
            }
        }
    }
}