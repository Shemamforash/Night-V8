using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using EZCameraShake;
using Facilitating;
using Facilitating.UIControllers;
using Fastlights;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Animals;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear.Weapons;
using Game.Global;
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
//        private Image _vignetteRenderer;

        private bool _dashPressed;

        private CharacterCombat _lockedTarget;
        private const float RotateSpeedMax = 100f;
        private float _rotateSpeedCurrent;
        private const float RotateAcceleration = 400f;
        private bool _recovered;
        public List<Action<Shot>> OnFireActions = new List<Action<Shot>>();
        public List<Action> UpdateSkillActions = new List<Action>();
        public BaseWeaponBehaviour _weaponBehaviour;
        private FastLight _muzzleFlash;

        public bool DamageTakenSinceLastShot;

        public bool ConsumeAdrenaline(int amount)
        {
            if (amount > _adrenalineLevel.CurrentValue()) return false;
            _adrenalineLevel.SetCurrentValue(0);
            RageBarController.SetRageBarFill(_adrenalineLevel.Normalised());
            return true;
        }

        private void Move(Vector2 direction)
        {
            if (CanDash())
            {
                MovementController.Dash(direction);
                _dashCooldown.Start();
                _dashPressed = false;
            }
            else
            {
                MovementController.Move(direction);
            }
        }

        public override void Awake()
        {
            base.Awake();
            Instance = this;
//            _vignetteRenderer = GameObject.Find("Vignette").GetComponent<Image>();
        }

        protected override UIHealthBarController HealthBarController()
        {
            return PlayerUi.Instance().GetHealthController(this);
        }

        protected override UIArmourController ArmourBarController()
        {
            return PlayerUi.Instance().GetArmourController(Player);
        }

        protected override int GetBurnDamage()
        {
            int burnDamage = base.GetBurnDamage();
            burnDamage = (int) (burnDamage * (Player.Attributes.FireDamageModifier + 1f));
            return burnDamage;
        }

        protected override int GetDecayDamage()
        {
            int decayDamage = base.GetDecayDamage();
            decayDamage = (int) (decayDamage * (Player.Attributes.DecayDamageModifier + 1f));
            return decayDamage;
        }

        protected override int GetSicknessTargetTicks()
        {
            int sicknessTargetTicks = base.GetSicknessTargetTicks();
            sicknessTargetTicks = (int) (sicknessTargetTicks - Player.Attributes.SicknessStackModifier);
            return sicknessTargetTicks;
        }

        //input
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
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
                    case InputAxis.Lock:
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
                        UiAreaInventoryController.OpenInventory();
                        break;
                    case InputAxis.TakeItem:
                        UiAreaInventoryController.TakeItem();
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
            Player.Attributes.Get(AttributeType.Perception).Decrement();
            UiCompassPulseController.UpdateCompassPulses();
            --_compassPulses;
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.Fire:
                    _weaponBehaviour.StopFiring();
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

        private void FollowTarget()
        {
            if (_lockedTarget != null && !Helper.OnScreen(_lockedTarget.gameObject)) _lockedTarget = null;
            if (_lockedTarget == null) return;
            float rotation = AdvancedMaths.AngleFromUp(transform.position, _lockedTarget.transform.position);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        }

        private void LockTarget()
        {
            if (_currentTarget == null) return;
            _lockedTarget = _lockedTarget == null ? _currentTarget : null;
            TargetBehaviour.SetLocked(_lockedTarget != null);
        }

        private void Rotate(float direction)
        {
            if (_lockedTarget != null) return;
            _rotateSpeedCurrent += RotateAcceleration * Time.deltaTime;
            if (_rotateSpeedCurrent > RotateSpeedMax) _rotateSpeedCurrent = RotateSpeedMax;
            transform.Rotate(Vector3.forward, _rotateSpeedCurrent * Time.deltaTime * (-direction).Polarity());
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

        public override void Kill()
        {
            if (!_recovered)
            {
                float recoverAmount = Player.Attributes.RallyHealthModifier;
                int healAmount = Mathf.FloorToInt(HealthController.GetMaxHealth() * recoverAmount);
                if (healAmount != 0)
                {
                    HealthController.Heal(healAmount);
                    _recovered = true;
                    return;
                }
            }

            InputHandler.SetCurrentListener(null);
            InputHandler.UnregisterInputListener(this);
            base.Kill();
            Player.Kill();
            bool isWanderer = Player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer;
            CombatManager.ExitCombat(!isWanderer);
            if (!isWanderer) return;
            SceneChanger.ChangeScene("Game Over");
        }

        public override void Update()
        {
            if (!CombatManager.InCombat()) return;
            base.Update();
            UpdateSkillActions.ForEach(a => a());
            FollowTarget();
            TransitionOffScreen();
            UpdateMuzzleFlash();
        }

        public void UpdateAdrenaline(int damageDealt)
        {
            _adrenalineLevel.Increment(damageDealt / 500f);
            CombatManager.IncreaseDamageDealt(damageDealt);
            DamageDealtSinceMarkStarted += damageDealt;
            RageBarController.SetRageBarFill(_adrenalineLevel.Normalised());
        }

        private void UpdateMuzzleFlash()
        {
            MuzzleFlashOpacity -= Time.deltaTime;
            if (MuzzleFlashOpacity < 0) MuzzleFlashOpacity = 0;
            Color c = _muzzleFlash.Colour;
            if (c.a == 0 && MuzzleFlashOpacity == 0) return;
            c.a = MuzzleFlashOpacity;
            _muzzleFlash.Colour = c;
        }

        public override void TakeShotDamage(Shot shot)
        {
            base.TakeShotDamage(shot);
            UpdateSkillActions.Clear();
            _damageTakenSinceMarkStarted = true;
            DamageTakenSinceLastShot = true;
            CombatManager.IncreaseDamageTaken(shot.DamageDealt());
            TryExplode();
        }

        public override void TakeExplosionDamage(float damage, Vector2 direction)
        {
            base.TakeExplosionDamage(damage, direction);
            TryExplode();
        }

        public override void TakeRawDamage(float damage, Vector2 direction)
        {
            base.TakeRawDamage(damage, direction);
            TryExplode();
        }

        private void TryExplode()
        {
            bool explodeWithFire = Random.Range(0f, 1f) < Player.Attributes.FireExplodeChance;
            bool explodeWithDecay = Random.Range(0f, 1f) < Player.Attributes.DecayExplodeChance;
            if (!explodeWithFire && !explodeWithDecay) return;
            Explosion explosion = Explosion.CreateExplosion(transform.position, 20);
            explosion.AddIgnoreTarget(this);
            if (explodeWithFire && explodeWithDecay)
            {
                if (Random.Range(0, 2) == 0)
                    explosion.SetIncendiary();
                else
                    explosion.SetDecay();
            }
            else if (explodeWithFire)
                explosion.SetIncendiary();
            else if (explodeWithDecay)
                explosion.SetDecay();

            explosion.InstantDetonate();
        }

        public void EquipWeapon(Weapon weapon)
        {
            Player.EquipWeapon(weapon);
            Destroy(_weaponBehaviour);
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
        }

        public void ResetCompass()
        {
            _compassPulses = Player.Attributes.CalculateCompassPulses();
            UiCompassPulseController.InitialisePulses(_compassPulses);
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

            ArmourBarController().TakeDamage(ArmourController);
            MovementController.SetSpeed(Player.Attributes.CalculateSpeed());

            _adrenalineGain = Player.Attributes.CalculateAdrenalineRecoveryRate();
            _skillCooldownModifier = Player.Attributes.CalculateSkillCooldownModifier();
            ResetCompass();

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

            HealthController.SetInitialHealth(Player.Attributes.CalculateInitialHealth(), this, Player.Attributes.CalculateMaxHealth());
            HealthController.AddOnHeal(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));

            HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue());

            _playerLight = GameObject.Find("Player Light").GetComponent<FastLight>();
            _playerLight.Radius = CombatManager.VisibilityRange();
//            _vignetteRenderer.material.SetFloat("_ViewDistance", CombatManager.VisibilityRange());

            SkillBar.BindSkills(Player, _skillCooldownModifier);
            UIMagazineController.SetWeapon(_weaponBehaviour);
            transform.position = PathingGrid.GetEdgeCell().Position;
            float zRot = AdvancedMaths.AngleFromUp(transform.position, Vector2.zero);
            transform.rotation = Quaternion.Euler(0f, 0f, zRot);
        }

        public override float GetAccuracyModifier()
        {
            return _weaponBehaviour is AccuracyGainer ? Mathf.Sqrt(1 - base.GetAccuracyModifier()) : base.GetAccuracyModifier();
        }

        public void Shake(float dps)
        {
            float magnitude = dps / 100f;
            if (magnitude > 1) magnitude = 1f;
            CameraShaker.Instance.ShakeOnce(magnitude, 10, 0.2f, 0.2f);
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished() && _dashPressed && ConsumeAdrenaline(2);
        }

        public void Knockback(Vector3 source, float force = 10f)
        {
            MovementController.Knockback(source, force);
            StopReloading();
        }

        public void SetTarget(EnemyBehaviour e)
        {
            if (_lockedTarget != null) return;
            if (e != null && !Helper.OnScreen(e.gameObject)) return;
            Flit flit = e as Flit;
            if (flit != null && !flit.Discovered()) return;
            TargetBehaviour.SetTarget(e == null ? null : e.transform);
            _currentTarget = e;
            EnemyUi.Instance().SetSelectedEnemy(e);
        }

        public override Weapon Weapon() => Player.EquippedWeapon;

        public void ExitCombat()
        {
            StopReloading();
            Player.Attributes.CalculateNewStrength(HealthController.GetCurrentHealth());
        }

        //RELOADING
        private void Reload()
        {
            if (_reloadingCoroutine != null) return;
            if (_weaponBehaviour.FullyLoaded()) return;
            if (!_weaponBehaviour.CanReload()) return;
            _reloadingCoroutine = StartCoroutine(StartReloading());
            _dryFireTimer = 0f;
        }

        private void StopReloading()
        {
            if (_reloadingCoroutine != null) StopCoroutine(_reloadingCoroutine);
            _reloadingCoroutine = null;
            UIMagazineController.UpdateMagazineUi();
            _reloading = false;
        }

        private bool _reloading;
        public int DamageDealtSinceMarkStarted;
        private bool _damageTakenSinceMarkStarted;
        private float _dryFireTimer;
        public const float DryFireTimerMax = 0.3f;

        //COOLDOWNS

        private void InstantReload()
        {
            _weaponBehaviour.Reload();
            StopReloading();
        }

        private IEnumerator StartReloading()
        {
            float duration = Player.EquippedWeapon.GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            _reloading = true;
            WeaponAudio.StartReload(Weapon().WeaponType());

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

            float reloadFailChance = Player.Attributes.ReloadFailureChance;
            if (Random.Range(0f, 1f) >= reloadFailChance)
            {
                _weaponBehaviour.Reload();
                OnFireActions.Clear();
                WeaponAudio.StopReload(Weapon().WeaponType());
            }

            StopReloading();
        }

        public override void ApplyShotEffects(Shot s)
        {
            OnFireActions.ForEach(a => a.Invoke(s));
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

            if (_weaponBehaviour.Empty()) TryDryFire();
            if (!_weaponBehaviour.CanFire()) return;
            _weaponBehaviour.StartFiring();
            CombatManager.SetHasFiredShot();
        }

        private void TryDryFire()
        {
            if (_dryFireTimer < DryFireTimerMax)
            {
                _dryFireTimer += Time.deltaTime;
                return;
            }

            _dryFireTimer = 0f;
            WeaponAudio.DryFire();
        }

        public void OnShotConnects(ITakeDamageInterface hit)
        {
            if (!Player.Attributes.ReloadOnLastRound || !_weaponBehaviour.Empty() || !hit.IsDead()) return;
            InstantReload();
        }

        //MISC

        public override CharacterCombat GetTarget() => _currentTarget;

        public static void Equip(Weapon weapon)
        {
        }

        public void ConsumeAmmo(int amount = -1)
        {
            _weaponBehaviour.ConsumeAmmo(amount);
        }

        public void StartMark(EnemyBehaviour target)
        {
            target.Mark();
            DamageDealtSinceMarkStarted = 0;
            _damageTakenSinceMarkStarted = false;
        }

        public void EndMark(EnemyBehaviour target)
        {
            if (_damageTakenSinceMarkStarted) return;
            target.HealthController.TakeDamage(DamageDealtSinceMarkStarted);
        }

        public void TriggerEnemyDeathEffect()
        {
            int damage = Mathf.FloorToInt(HealthController.GetMaxHealth() * Player.Attributes.EnemyKillHealthLoss);
            TakeRawDamage(damage, Vector2.zero);
            if (Random.Range(0f, 1f) >= Player.Attributes.InstantCooldownChance) return;
            SkillBar.ResetCooldowns();
        }
    }
}