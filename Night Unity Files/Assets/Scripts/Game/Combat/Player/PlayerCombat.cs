using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using DG.Tweening;
using Facilitating;
using Facilitating.UIControllers;
using Fastlights;
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
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
    public class PlayerCombat : CharacterCombat, IInputListener
    {
        private EnemyBehaviour _currentTarget;
        private float  _skillCooldownModifier, _adrenalineGain;
        private readonly Number _adrenalineLevel = new Number(0, 0, 8);
        private Cooldown _dashCooldown;
        private int _initialArmour;
        private Quaternion _lastTargetRotation;
        private int _compassPulses;

        private Coroutine _reloadingCoroutine;

        private Number _strengthText;

        public Characters.Player Player;

        private float _reloadPressedTime;
        private const float MaxReloadPressedTime = 1f;
        private bool _inCombat;
        public static PlayerCombat Instance;
        public float MuzzleFlashOpacity { get; set; }

        private bool ConsumeAdrenaline(int amount)
        {
            if (amount > _adrenalineLevel.CurrentValue()) return false;
            _adrenalineLevel.Decrement(amount);
            RageBarController.SetRageBarFill(_adrenalineLevel.Normalised());
            return true;
        }

        protected override void Move(Vector2 direction)
        {
            if (CanDash())
            {
                Dash(direction);
                _dashPressed = false;
            }
            else
            {
                base.Move(direction);
            }
        }

        private void SetInCombat(bool inCombat)
        {
            _inCombat = inCombat;
            if (_inCombat)
            {
                PlayerUi.Instance().Show();
            }
            else
            {
                PlayerUi.Instance().Hide();
            }
        }

        public override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        //input
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (IsImmobilised) return;
            if (isHeld)
            {
                switch (axis)
                {
                    case InputAxis.Fire:
                        if (CombatManager.EnemiesOnScreen().Count == 0) UiAreaInventoryController.SetNearestContainer(_lastNearestContainer);
                        if (_inCombat) FireWeapon();
                        break;
                    case InputAxis.Horizontal:
                        Move(direction * transform.right);
                        break;
                    case InputAxis.Vertical:
                        Move(direction * transform.up);
                        break;
                    case InputAxis.SwitchTab:
                        Rotate(direction);
                        break;
                    case InputAxis.Reload:
                        float _lastPressedTime = _reloadPressedTime;
                        _reloadPressedTime += Time.deltaTime;
                        if (_reloadPressedTime >= MaxReloadPressedTime && _lastPressedTime < MaxReloadPressedTime)
                        {
                            SetInCombat(!_inCombat);
                        }

                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case InputAxis.Fire:
                        if (!_inCombat) TryEmitPulse();
                        break;
                    case InputAxis.Enrage:
                        if (_inCombat) LockTarget();
                        break;
                    case InputAxis.Reload:
                        if (_inCombat) Reload();
                        break;
                    case InputAxis.SkillOne:
                        if (_inCombat) SkillBar.ActivateSkill(0);
                        break;
                    case InputAxis.SkillTwo:
                        if (_inCombat) SkillBar.ActivateSkill(1);
                        break;
                    case InputAxis.SkillThree:
                        if (_inCombat) SkillBar.ActivateSkill(2);
                        break;
                    case InputAxis.SkillFour:
                        if (_inCombat) SkillBar.ActivateSkill(3);
                        break;
                    case InputAxis.Sprint:
                        _dashPressed = true;
                        break;
                }
            }
        }

        private void TryEmitPulse()
        {
            if (_compassPulses == 0) return;
            UiCompassController.EmitPulse();
            --_compassPulses;
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.Fire:
                    _weaponBehaviour.EndFiring();
                    break;
                case InputAxis.SwitchTab:
                    _rotateSpeedCurrent = 0f;
                    break;
                case InputAxis.Sprint:
                    _dashPressed = false;
                    break;
                case InputAxis.Reload:
                    _reloadPressedTime = 0f;
                    break;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        private bool _dashPressed;

        private CharacterCombat _lockedTarget;

        private void FollowTarget()
        {
            if (_lockedTarget != null && !Helper.IsObjectInCameraView(_lockedTarget.gameObject)) _lockedTarget = null;
            if (_lockedTarget == null) return;
            float rotation = AdvancedMaths.AngleFromUp(transform.position, _lockedTarget.transform.position);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        }

        private void LockTarget()
        {
            if (_lockedTarget == null)
            {
                _lockedTarget = GetTarget();
            }
            else
            {
                _lockedTarget = null;
            }
        }

        private const float RotateSpeedMax = 100f;
        private float _rotateSpeedCurrent;
        private const float RotateAcceleration = 400f;

        private void Rotate(float direction)
        {
            if (CanDash())
            {
                Spin(direction);
                _dashPressed = false;
                return;
            }

            if (_lockedTarget != null) return;
            _rotateSpeedCurrent += RotateAcceleration * Time.deltaTime;
            if (_rotateSpeedCurrent > RotateSpeedMax) _rotateSpeedCurrent = RotateSpeedMax;
            transform.Rotate(Vector3.forward, _rotateSpeedCurrent * Time.deltaTime * Helper.Polarity(-direction));
        }

        private void TransitionOffScreen()
        {
            float playerDistance = Vector2.Distance(Vector2.zero, transform.position);
            if (playerDistance < PathingGrid.CombatMovementDistance / 2f) return;
            playerDistance -= PathingGrid.CombatMovementDistance / 2f;
            float alpha = Mathf.Clamp(playerDistance, 0, 1);
            GameObject.Find("Screen Fader").GetComponent<Image>().color = new Color(0, 0, 0, alpha);
            if (alpha != 1) return;
            InputHandler.SetCurrentListener(null);
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            CombatManager.ExitCombat();
        }

        private void Spin(float direction)
        {
            transform.DORotate(new Vector3(0, 0, 180f * -direction), 0.5f, RotateMode.FastBeyond360).SetRelative();
        }

        public event Action<Shot> OnFireAction;
        public event Action OnReloadAction;

        public override void Kill()
        {
            base.Kill();
            Player.Kill();
            CombatManager.ExitCombat();
        }

        private void CheckForEnemiesOnScreen()
        {
//            PlayerUi.Instance().SetAlpha(CombatManager.AllEnemiesDead() ? 0 : 1);
        }

        public override void Update()
        {
            base.Update();
            FollowTarget();
            TransitionOffScreen();
            CheckForContainersNearby();
            CheckForEnemiesOnScreen();
            _adrenalineLevel.Increment(_adrenalineGain);
            UpdateMuzzleFlash();
        }

        private FastLight _muzzleFlash;
        
        private void UpdateMuzzleFlash()
        {
            MuzzleFlashOpacity -= Time.deltaTime;
            if (MuzzleFlashOpacity < 0) MuzzleFlashOpacity = 0;
            Color c = _muzzleFlash.Colour;
            if (c.a == 0 && MuzzleFlashOpacity == 0) return;
            c.a = MuzzleFlashOpacity;
            _muzzleFlash.Colour = c;
        }

        private const float MaxShowInventoryDistance = 0.5f;
        private ContainerController _lastNearestContainer;

        private void CheckForContainersNearby()
        {
            ContainerController nearestContainer = null;
            float nearestContainerDistance = MaxShowInventoryDistance;
            ContainerController.Containers.ForEach(c =>
            {
                float distance = Vector2.Distance(c.transform.position, PlayerCombat.Instance.transform.position);
                if (distance > nearestContainerDistance) return;
                nearestContainerDistance = distance;
                nearestContainer = c.ContainerController;
            });
            _lastNearestContainer = nearestContainer;
        }

        public void Initialise()
        {
            InputHandler.SetCurrentListener(this);

            _muzzleFlash = GameObject.Find("Muzzle Flash").GetComponent<FastLight>();
            Player = CharacterManager.SelectedCharacter;
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
            ArmourController = Player.ArmourController;
            _skillCooldownModifier = Player.Attributes.CalculateSkillCooldownModifier();
            _initialArmour = Player.ArmourController.GetProtectionLevel();

            Speed = Player.Attributes.CalculateSpeed();

            _adrenalineGain = Player.Attributes.CalculateAdrenalineRecoveryRate();
            _skillCooldownModifier = Player.Attributes.CalculateSkillCooldownModifier();
            _compassPulses = Player.Attributes.CalculateCompassPulses();
            
            _dashCooldown = CombatManager.CreateCooldown();
            _dashCooldown.Duration = 1;
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

            _adrenalineLevel.SetCurrentValue(0f);

            HealthController.SetInitialHealth(Player.Attributes.CalculateCombatHealth(), this);
            HealthController.AddOnHeal(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));

            HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue());

            SkillBar.BindSkills(Player);
            UIMagazineController.SetWeapon(_weaponBehaviour);
            SetInCombat(false);
            transform.position = PathingGrid.FindCellToAttackPlayer(CurrentCell(), PathingGrid.CombatAreaWidth, PathingGrid.CombatAreaWidth - 4).Position;
        }

        public override float GetAccuracyModifier()
        {
            if (_weaponBehaviour is AccuracyGainer) return 1 - base.GetAccuracyModifier();
            return base.GetAccuracyModifier();
        }

        private BaseWeaponBehaviour _weaponBehaviour;

        protected override void Dash(Vector2 direction)
        {
            base.Dash(direction);
            _dashCooldown.Start();
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished() && _dashPressed && ConsumeAdrenaline(2);
        }

        public override void Knockback(Vector3 source, float force = 10f)
        {
            base.Knockback(source, force);
            StopReloading();
        }

        public void SetTarget(EnemyBehaviour e)
        {
            if (_lockedTarget != null) return;
            if (e != null && !Helper.IsObjectInCameraView(e.gameObject)) return;
            TargetBehaviour.SetTarget(e == null ? null : e.transform);
            _currentTarget = e;
            EnemyUi.Instance().SetSelectedEnemy(e);
            
        }

        public override Weapon Weapon() => Player.Weapon;

        public void ExitCombat()
        {
            StopReloading();
            Player.Attributes.DecreaseWillpower();
        }

        //RELOADING
        private void Reload()
        {
            if (_reloadingCoroutine != null) return;
            if (_weaponBehaviour.FullyLoaded()) return;
            if (!_weaponBehaviour.CanReload()) return;
            _reloadingCoroutine = StartCoroutine(StartReloading());
        }

        private void StopReloading()
        {
            if (_reloadingCoroutine != null) StopCoroutine(_reloadingCoroutine);
            _reloadingCoroutine = null;
            UIMagazineController.UpdateMagazineUi();
            _reloading = false;
        }

        private bool _reloading;

        //COOLDOWNS

        private IEnumerator StartReloading()
        {
            float duration = Player.Weapon.GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            OnFireAction = null;
            _reloading = true;

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

            _weaponBehaviour.Reload();
            OnReloadAction?.Invoke();
            StopReloading();
        }


        public override void ApplyShotEffects(Shot s)
        {
            OnFireAction?.Invoke(s);
        }

        //FIRING
        public void FireWeapon()
        {
            if (_reloading) return;
            if (!_weaponBehaviour.CanFire()) return;
            _weaponBehaviour.StartFiring(this);
        }

        //MISC

        public override CharacterCombat GetTarget() => _currentTarget;
    }
}