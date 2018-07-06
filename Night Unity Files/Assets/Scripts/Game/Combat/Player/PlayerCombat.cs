using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using Facilitating;
using Facilitating.UIControllers;
using Fastlights;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Animals;
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
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
    public class PlayerCombat : CharacterCombat, IInputListener
    {
        private EnemyBehaviour _currentTarget;
        private float _skillCooldownModifier, _adrenalineGain;
        private readonly Number _adrenalineLevel = new Number(0, 0, 8);
        private Cooldown _dashCooldown;
        private int _initialArmour;
        private Quaternion _lastTargetRotation;
        private int _compassPulses;

        private Coroutine _reloadingCoroutine;

        private Number _strengthText;

        public Characters.Player Player;
        public FastLight _playerLight;

        private const float MaxReloadPressedTime = 1f;
        public static PlayerCombat Instance;
        public float MuzzleFlashOpacity;

        public bool ConsumeAdrenaline(int amount)
        {
            if (amount > _adrenalineLevel.CurrentValue()) return false;
            _adrenalineLevel.SetCurrentValue(0);
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

        public override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override int GetBurnDamage()
        {
            int burnDamage = base.GetBurnDamage();
            if (Player.Attributes.BurnWeakness) burnDamage *= 2;
            return burnDamage;
        }

        protected override int GetDecayDamage()
        {
            int decayDamage = base.GetDecayDamage();
            if (Player.Attributes.DecayWeakness) decayDamage *= 2;
            return decayDamage;
        }

        protected override int GetSicknessTargetTicks()
        {
            int sicknessTargetTicks = base.GetSicknessTargetTicks();
            if (Player.Attributes.SicknessWeakness) sicknessTargetTicks /= 2;
            return sicknessTargetTicks;
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
                        FireWeapon();
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
                }
            }
            else
            {
                switch (axis)
                {
                    case InputAxis.Enrage:
                        LockTarget();
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
                    case InputAxis.Sprint:
                        _dashPressed = true;
                        break;
                    case InputAxis.Inventory:
                        UiAreaInventoryController.Instance().OpenInventory();
                        break;
                    case InputAxis.Compass:
                        TryEmitPulse();
                        break;
                }
            }
        }

        private void TryEmitPulse()
        {
            if (_compassPulses == 0) return;
            if (!UiCompassController.EmitPulse()) return;
            UiCompassPulseController.UpdateCompassPulses();
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
        private bool _recovered;

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
            if (!_recovered)
            {
                float recoverAmount = Player.Attributes.Val(AttributeType.HealthRecoveryBonus);
                int healAmount = Mathf.FloorToInt(HealthController.GetMaxHealth() * recoverAmount);
                if (healAmount != 0)
                {
                    HealthController.Heal(healAmount);
                    _recovered = true;
                    return;
                }
            }

            base.Kill();
            Player.Kill();
            CombatManager.ExitCombat();
        }

        public override void Update()
        {
            if (!CombatManager.InCombat()) return;
            base.Update();
            FollowTarget();
            TransitionOffScreen();
            _adrenalineLevel.Increment(_adrenalineGain * Time.deltaTime);
            RageBarController.SetRageBarFill(_adrenalineLevel.Normalised());
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

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            CombatManager.IncreaseDamageTaken(shot.DamageDealt());
            if (!Player.Attributes.DecayRetaliate) return;
            EnemyBehaviour b = shot._origin as EnemyBehaviour;
            if (b == null) return;
            b.Decay();
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

            CharacterUi.GetArmourController(Player).TakeDamage(ArmourController);
            Speed = Player.Attributes.CalculateSpeed();

            _adrenalineGain = Player.Attributes.CalculateAdrenalineRecoveryRate();
            _skillCooldownModifier = Player.Attributes.CalculateSkillCooldownModifier();
            _compassPulses = Player.Attributes.CalculateCompassPulses();
            UiCompassPulseController.InitialisePulses(_compassPulses);

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

            _playerLight = GameObject.Find("Player Light").GetComponent<FastLight>();
            _playerLight.Radius = CombatManager.VisibilityRange();

            SkillBar.BindSkills(Player, _skillCooldownModifier);
            UIMagazineController.SetWeapon(_weaponBehaviour);
            //todo give the player a position;
            transform.position = Vector2.zero;
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
            return _dashCooldown.Finished() && _dashPressed;
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
            Flit flit = e as Flit;
            if (flit != null && !flit.Discovered()) return;
            TargetBehaviour.SetTarget(e == null ? null : e.transform);
            _currentTarget = e;
            EnemyUi.Instance().SetSelectedEnemy(e);
        }

        public override Weapon Weapon() => Player.Weapon;

        public void ExitCombat()
        {
            StopReloading();
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

        private void InstantReload()
        {
            _weaponBehaviour.Reload();
            OnReloadAction?.Invoke();
            StopReloading();
        }

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

            float reloadFailChance = Player.Attributes.Val(AttributeType.ReloadFailChance);
            if (Random.Range(0f, 1f) > reloadFailChance)
            {
                _weaponBehaviour.Reload();
            }

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
            if (_weaponBehaviour.Empty() && Player.Attributes.ReloadOnEmptyMag)
            {
                Reload();
                return;
            }

            if (!_weaponBehaviour.CanFire()) return;
            _weaponBehaviour.StartFiring(this);
            CombatManager.SetHasFiredShot();
        }

        public void OnShotConnects(CharacterCombat hit)
        {
            if (!Player.Attributes.ReloadOnLastRound || !_weaponBehaviour.Empty() || !hit.IsDead) return;
            InstantReload();
        }

        //MISC

        public override CharacterCombat GetTarget() => _currentTarget;
    }
}