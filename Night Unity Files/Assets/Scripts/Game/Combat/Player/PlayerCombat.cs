using System;
using System.Collections;
using System.Collections.Generic;
using EZCameraShake;
using Facilitating;
using Facilitating.UIControllers;
using Fastlights;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Animals;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
    public class PlayerCombat : CharacterCombat, IInputListener, ICombatEvent
    {
        private float _skillCooldownModifier;
        private readonly Number _adrenalineLevel = new Number(0, 0, 8);
        private Coroutine _dashCooldown;
        private Quaternion _lastTargetRotation;
        private int _compassPulses;

        private Coroutine _reloadingCoroutine;

        private Number _fettleText;

        public Characters.Player Player;
        public FastLight _playerLight;

        public static PlayerCombat Instance;

        public float MuzzleFlashOpacity;

        private bool _dashPressed;

        private CanTakeDamage _lockedOn;
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
            if (!CanAffordSkill(amount)) return false;
            Player.BrandManager.IncreaseAdrenalineUsed(amount);
            _adrenalineLevel.Decrement(amount);
            RageBarController.SetRageBarFill(_adrenalineLevel.Normalised());
            return true;
        }

        public bool CanAffordSkill(int amount)
        {
            return amount <= _adrenalineLevel.CurrentValue();
        }

        private void Move(Vector2 direction)
        {
            if (CanDash())
            {
                MovementController.Dash(direction);
                _dashCooldown = StartCoroutine(Dash());
                _dashPressed = false;
            }
            else
            {
                MovementController.Move(direction);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
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

        private void MoveVertical(float direction = 0)
        {
            if (CameraLock.IsCameraLocked()) Move(direction * transform.up);
            else Move(direction * Camera.main.transform.up);
        }

        private void MoveHorizontal(float direction = 0)
        {
            if (CameraLock.IsCameraLocked()) Move(direction * transform.right);
            else Move(direction * Camera.main.transform.right);
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
                        MoveHorizontal(direction);
                        break;
                    case InputAxis.Vertical:
                        MoveVertical(direction);
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
                        UiGearMenuController.ShowArmourMenu();
                        break;
                    case InputAxis.TakeItem:
                        EventTextController.Activate();
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
            Player.Attributes.Get(AttributeType.Focus).Decrement();
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
            if (_lockedOn == null) return;
            float rotation = AdvancedMaths.AngleFromUp(transform.position, _lockedOn.transform.position);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
        }

        private void LockTarget()
        {
            _lockedOn = _lockedOn == null ? GetTarget() : null;
        }

        private void Rotate(float direction)
        {
            if (_lockedOn != null) return;
            _rotateSpeedCurrent += RotateAcceleration * Time.deltaTime;
            if (_rotateSpeedCurrent > RotateSpeedMax) _rotateSpeedCurrent = RotateSpeedMax;
            transform.Rotate(Vector3.forward, _rotateSpeedCurrent * Time.deltaTime * (-direction).Polarity());
        }

        public float InRange()
        {
            return CurrentCell().OutOfRange ? 1 : -1;
        }

        public string GetEventText()
        {
            return "Leave region [T]";
        }

        public void Activate()
        {
            CombatManager.ExitCombat();
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            InputHandler.SetCurrentListener(null);
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
            SceneChanger.GoToGameOverScene();
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            UpdateSkillActions.ForEach(a => a());
            FollowTarget();
            UpdateMuzzleFlash();
        }

        public override string GetDisplayName()
        {
            return "Player";
        }

        public void UpdateAdrenaline(int damageDealt)
        {
            _adrenalineLevel.Increment(damageDealt / 150f * _adrenalineRecoveryRate);
            Player.BrandManager.IncreaseDamageDealt(damageDealt);
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
            Player.BrandManager.IncreaseDamageTaken(shot.DamageDealt());
            TryExplode();
        }

        public override void TakeExplosionDamage(int damage, Vector2 direction, float radius)
        {
            base.TakeExplosionDamage(damage, direction, radius);
            TryExplode();
            Shake(damage * 100);
        }

        public override void TakeRawDamage(int damage, Vector2 direction)
        {
            base.TakeRawDamage(damage, direction);
            TryExplode();
        }

        protected override void TakeDamage(int damage, Vector2 direction)
        {
            base.TakeDamage(damage, direction);
            Player.Attributes.CalculateNewFettle(HealthController.GetCurrentHealth());
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
                    explosion.SetBurn();
                else
                    explosion.SetDecay();
            }
            else if (explodeWithFire)
                explosion.SetBurn();
            else if (explodeWithDecay)
                explosion.SetDecay();

            explosion.InstantDetonate();
        }

        public void EquipWeapon(Weapon weapon)
        {
            Destroy(_weaponBehaviour);
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
            UIMagazineController.SetWeapon(_weaponBehaviour);
            RecalculateAttributes();
        }

        public void RecalculateAttributes()
        {
            int currentHealth = (int) HealthController.GetCurrentHealth();
            int maxHealth = Player.Attributes.CalculateMaxHealth();
            if (HealthController.GetNormalisedHealthValue() == 1) currentHealth = maxHealth;
            HealthController.SetInitialHealth(currentHealth, this, maxHealth);
            MovementController.SetSpeed(Player.Attributes.CalculateSpeed());
            _skillCooldownModifier = Player.Attributes.CalculateSkillCooldownModifier();
            SkillBar.BindSkills(Player, _skillCooldownModifier);
            _adrenalineRecoveryRate = Player.Attributes.CalculateAdrenalineRecoveryRate();
        }

        public void EquipArmour()
        {
            ArmourController = Player.ArmourController;
        }

        public void ResetCompass()
        {
            _compassPulses = Player.Attributes.CalculateCompassPulses();
            UiCompassPulseController.InitialisePulses(_compassPulses);
        }

        private IEnumerator Dash()
        {
            float duration = 1f;
            while (duration > 0f)
            {
                duration -= Time.deltaTime;
                if (duration < 0f) duration = 0f;
                RageBarController.UpdateDashTimer(duration);
                yield return null;
            }

            RageBarController.UpdateDashTimer(1);
            RageBarController.PlayFlash();
            _dashCooldown = null;
        }

        public void RecalculateHealth()
        {
            HealthController.SetInitialHealth(Player.Attributes.CalculateInitialHealth(), this, Player.Attributes.CalculateMaxHealth());
        }
        
        public void Initialise()
        {
            InputHandler.SetCurrentListener(this);

            _muzzleFlash = GameObject.Find("Muzzle Flash").GetComponent<FastLight>();
            Player = CharacterManager.SelectedCharacter;
            RecalculateHealth();
            EquipWeapon(Weapon());
            EquipArmour();
            ResetCompass();
            _adrenalineLevel.SetCurrentValue(0f);

            _playerLight = GameObject.Find("Player Light").GetComponent<FastLight>();
            _playerLight.Radius = CombatManager.VisibilityRange();

            SkillBar.BindSkills(Player, _skillCooldownModifier);
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
            return _dashCooldown == null && _dashPressed && ConsumeAdrenaline(1);
        }

        public override void SetTarget(CanTakeDamage target)
        {
            if (_lockedOn != null) return;
            if (target != null)
            {
                Flit flit = target as Flit;
                if (flit != null && !flit.Discovered()) return;
            }

            base.SetTarget(target);
        }

        public override Weapon Weapon() => Player.EquippedWeapon;

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
        private float _adrenalineRecoveryRate;
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
                ActiveSkillController.Stop();
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
            if (_weaponBehaviour.Empty())
            {
                if (Player.Attributes.ReloadOnEmptyMag) Reload();
                else TryDryFire();
                return;
            }

            if (!_weaponBehaviour.CanFire()) return;
            _weaponBehaviour.StartFiring();
            if (_weaponBehaviour.Empty()) ActiveSkillController.Stop();
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

        public void OnShotConnects(CanTakeDamage hit)
        {
            if (!Player.Attributes.ReloadOnLastRound || !_weaponBehaviour.Empty()) return;
            InstantReload();
        }

        //MISC

        public void ConsumeAmmo(int amount = -1)
        {
            _weaponBehaviour.ConsumeAmmo(amount);
        }

        public void StartMark(CanTakeDamage target)
        {
            target.Mark();
            DamageDealtSinceMarkStarted = 0;
            _damageTakenSinceMarkStarted = false;
        }

        public void EndMark(CanTakeDamage target)
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

        public bool IsTargetLocked()
        {
            return _lockedOn != null;
        }

        public void ReduceAdrenaline(float amount)
        {
            _adrenalineLevel.Decrement(amount);
        }
    }
}