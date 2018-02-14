using System;
using Assets;
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
        private float _damageModifier, _skillCooldownModifier, _initialHealth;
        private int _initialArmour;

        private const int PlayerHealthChunkSize = 50;

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
        private TextMeshProUGUI _coverText;

        public CanvasGroup PlayerCanvasGroup;
        private Number _strengthText;


        public override void Kill()
        {
            Player.Kill();
        }
        
        public override void Awake()
        {
            base.Awake();
            PlayerCanvasGroup = gameObject.GetComponent<CanvasGroup>();
            _playerName = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            _coverText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Cover");
            _coverText.text = "";
            HealthController.AddOnHeal(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
        }

        public void UpdatePlayerDirection()
        {
            if (CurrentTarget == null) return;
            FacingDirection = CurrentTarget.DistanceToPlayer > 0 ? Direction.Right : Direction.Left;
            UIEnemyController.Enemies.ForEach(e =>
            {
                if (FacingDirection == Direction.Left && e.DistanceToPlayer > 0)
                {
                    e.Hide();
                    return;
                }

                if (FacingDirection == Direction.Right && e.DistanceToPlayer < 0)
                {
                    e.Hide();
                    return;
                }

                e.Show();
            });
        }

        public override void SetPlayer(Character player)
        {
            base.SetPlayer(player);
            Player = (Player)player;
            _playerName.text = player.Name;
            Speed = Player.Attributes.Endurance.CurrentValue() * 0.4f + 1;
            MoveForwardAction = f => Position.Increment(f);
            MoveBackwardAction = f => Position.Decrement(f);
            Position.SetCurrentValue(0);
            _dashCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            float dashDuration = 2f - 0.1f * Player.Attributes.Endurance.CurrentValue();
            _dashCooldown.Duration = dashDuration;
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

            _damageModifier = (float) Math.Pow(1.05f, Player.Attributes.Perception.CurrentValue());
            _skillCooldownModifier = (float) Math.Pow(0.95f, Player.Attributes.Willpower.CurrentValue());
            int initialHealth = (int) (Player.Attributes.Strength.CurrentValue() * PlayerHealthChunkSize);
            HealthController.SetInitialHealth(initialHealth, this);
            _initialArmour = player.Armour?.ArmourRating ?? 0;

            HeartBeatController.Enable();
            HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue());
            RecoilManager.EnterCombat();
            HealthController.SetIsPlayerBar();

            RageController = new RageController();
            Position.AddOnValueChange(a => { UIEnemyController.Enemies.ForEach(e => e.Position.UpdateValueChange()); });
            SetReloadCooldown();
            ArmourController.SetArmourValue(_initialArmour);
            RageController.EnterCombat();
            SkillBar.BindSkills(Player);
            FacingDirection = Direction.Right;
            InputHandler.RegisterInputListener(this);
            UIMagazineController.SetWeapon(Weapon());
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

        public void OnHit(Shot shot, int damage)
        {
            OnHit(damage, shot.IsCritical);
            if (shot.Origin() == null) return;
            if (Retaliate) FireWeapon(shot.Origin());
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
            PlayerCanvasGroup.alpha = 0.4f;
            _coverText.text = "In Cover";
        }

        protected override void LeaveCover()
        {
            base.LeaveCover();
            PlayerCanvasGroup.alpha = 1f;
            _coverText.text = "Exposed";
        }

        public void SetTarget(DetailedEnemyCombat e)
        {
            if (e == null) return;
            CurrentTarget = e;
            CurrentTarget.MarkSelected();
        }

        //MISC
        protected override bool Immobilised()
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
        protected override Shot FireWeapon(CharacterCombat target)
        {
            Shot shot = base.FireWeapon(target);
            if (shot != null)
            {
                if (RageController.Active()) shot.GuaranteeCritical();
                shot.SetDamageModifier(_damageModifier);
                OnFireAction?.Invoke(shot);
                UpdateMagazineUi();
                shot.Fire();
                _fired = true;
            }

            return shot;
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
            switch (axis)
            {
                case InputAxis.Cover:
                    if (!isHeld) ChangeCover();
                    break;
                case InputAxis.Enrage:
                    RageController.TryStart();
                    break;
                case InputAxis.Fire:
                    if (!_fired && isHeld) break;
                    if (!_fired || Player.Weapon.WeaponAttributes.Automatic) FireWeapon(CurrentTarget);
                    break;
                case InputAxis.Reload:
                    Reload();
                    break;
                case InputAxis.Horizontal:
                    Move(direction);
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