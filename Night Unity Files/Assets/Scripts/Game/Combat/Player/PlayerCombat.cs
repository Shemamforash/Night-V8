using System;
using System.Collections;
using System.Collections.Generic;
using EZCameraShake;
using Facilitating;
using Facilitating.UIControllers;
using Fastlights;
using Game.Characters;
using Game.Combat.Enemies.Animals;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Ui;
using Game.Exploration.Regions;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;
using static Game.Combat.Player.PlayerCombat;
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
    public class PlayerCombat : CharacterCombat, IInputListener, ICombatEvent
    {
        private bool _reloading;
        private float _dryFireTimer;
        private float _adrenalineRecoveryRate;
        private const float DryFireTimerMax = 0.3f;
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

        private const float RotateSpeedMax = 150f;
        private float _rotateSpeedCurrent;
        private const float RotateAcceleration = 400f;
        private bool _recovered;
        public List<Action<Shot>> OnFireActions = new List<Action<Shot>>();
        public List<Action> UpdateSkillActions = new List<Action>();
        public BaseWeaponBehaviour _weaponBehaviour;
        private FastLight _muzzleFlash;
        private DeathReason _currentDeathReason;

        private bool _useKeyboardMovement = true;
        private Vector2? _lastMousePosition;
        private Camera _mainCamera;
        private const float EnemyDamageModifier = 0.2f;


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
            IsPlayer = true;
            base.Awake();
            Instance = this;
            _mainCamera = Camera.main;
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
            if (Player.Attributes.TakeDoubleDecayDamage && Helper.RollDie(0, 4)) decayDamage *= 2;
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
            if (_useKeyboardMovement) Move(direction * transform.up);
            else Move(direction * _mainCamera.transform.up);
        }

        private void MoveHorizontal(float direction = 0)
        {
            if (_useKeyboardMovement) Move(direction * transform.right);
            else Move(direction * _mainCamera.transform.right);
        }

        //input
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld)
            {
                switch (axis)
                {
                    case InputAxis.Mouse:
                        FireWeapon();
                        break;
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
                        UiGearMenuController.ShowInventories();
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
            --_compassPulses;
            UiCompassPulseController.UsePulse(_compassPulses);
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.Mouse:
                    _weaponBehaviour.StopFiring();
                    break;
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

        private void Rotate(float direction)
        {
            _useKeyboardMovement = true;
            _rotateSpeedCurrent += RotateAcceleration * Time.deltaTime;
            if (_rotateSpeedCurrent > RotateSpeedMax) _rotateSpeedCurrent = RotateSpeedMax;
            transform.Rotate(Vector3.forward, _rotateSpeedCurrent * Time.deltaTime * (-direction).Polarity());
        }

        private void UpdateRotation()
        {
            Vector2 mouseScreenPosition = Input.mousePosition;
            bool ignoreMouseRotation = _lastMousePosition == null || _useKeyboardMovement && mouseScreenPosition == _lastMousePosition.Value;
            _lastMousePosition = mouseScreenPosition;
            if (ignoreMouseRotation) return;
            _useKeyboardMovement = false;
            Vector2 mousePosition = Helper.MouseToWorldCoordinates();
            float rotation = AdvancedMaths.AngleFromUp(transform.position, mousePosition);
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }

        public float InRange()
        {
            if (CombatManager.GetCurrentRegion().GetRegionType() == RegionType.Rite) return -1;
            return CurrentCell().OutOfRange ? 1 : -1;
        }

        public string GetEventText()
        {
            return "Leave region [T]";
        }

        public override bool Burn()
        {
            if (!base.Burn()) return false;
            Player.BrandManager.IncreaseBurnCount(GetBurnDamage());
            _currentDeathReason = DeathReason.Fire;
            return true;
        }

        public override bool Sicken(int stacks = 1)
        {
            if (!base.Sicken(stacks)) return false;
            _currentDeathReason = DeathReason.Sickness;
            Instance.Player.BrandManager.IncreaseSickenCount();
            return true;
        }

        public override void Decay()
        {
            int armourCountBefore = ArmourController.GetCurrentProtection();
            base.Decay();
            int armourCountAfter = ArmourController.GetCurrentProtection();
            if (armourCountBefore - armourCountAfter == 0) return;
            Instance.Player.BrandManager.IncreaseDecayCount();
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

            if (CombatManager.GetCurrentRegion().GetRegionType() == RegionType.Rite)
            {
                ShrineBehaviour.ActiveShrine.Fail();
                return;
            }

            InputHandler.SetCurrentListener(null);
            InputHandler.UnregisterInputListener(this);
            base.Kill();
            bool isWanderer = Player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer;
            if (!isWanderer) CombatManager.ExitCombat();
            Player.Kill(_currentDeathReason);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            UpdateSkillActions.ForEach(a => a());
            UpdateMuzzleFlash();
            UpdateRotation();
        }

        public override string GetDisplayName()
        {
            return "Player";
        }

        public void UpdateAdrenaline(int damageDealt)
        {
            _adrenalineLevel.Increment(damageDealt / 300f * _adrenalineRecoveryRate);
            Player.BrandManager.IncreaseDamageDealt(damageDealt);
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
            _currentDeathReason = DeathReason.Standard;
            base.TakeShotDamage(shot);
            UpdateSkillActions.Clear();
        }

        public override void TakeExplosionDamage(int damage, Vector2 direction, float radius)
        {
            _currentDeathReason = DeathReason.Standard;
            base.TakeExplosionDamage(damage, direction, radius);
            Shake(damage * 100);
        }

        protected override void TakeDamage(int damage, Vector2 direction)
        {
            damage = (int) (damage * EnemyDamageModifier);
            if (damage < 1) damage = 1;
            Player.BrandManager.IncreaseDamageTaken(damage);
            base.TakeDamage(damage, direction);
            TryExplode();
            Player.Attributes.CalculateNewFettle(HealthController.GetCurrentHealth());
        }

        private void TryExplode()
        {
            bool explodeWithFire = Random.Range(0f, 1f) < Player.Attributes.FireExplodeChance;
            bool explodeWithDecay = Random.Range(0f, 1f) < Player.Attributes.DecayExplodeChance;
            if (!explodeWithFire && !explodeWithDecay) return;
            if (explodeWithFire && explodeWithDecay)
            {
                explodeWithFire = Helper.RollDie(0, 2);
                explodeWithDecay = !explodeWithFire;
            }

            if (explodeWithDecay) DecayBehaviour.Create(transform.position).AddIgnoreTarget(this);
            else FireBurstBehaviour.Create(transform.position).AddIgnoreTarget(this);
        }

        public void EquipWeapon(Weapon weapon)
        {
            Destroy(_weaponBehaviour);
            _weaponBehaviour = Weapon().InstantiateWeaponBehaviour(this);
            UIMagazineController.SetWeapon(_weaponBehaviour);
            RecalculateAttributes();
        }

        public void EquipInscription()
        {
            UIMagazineController.SetWeapon(_weaponBehaviour);
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
            int compassBonus = Mathf.CeilToInt(Player.Attributes.Val(AttributeType.CompassBonus));
            int focusMax = Mathf.CeilToInt(Player.Attributes.Max(AttributeType.Focus)) + compassBonus;
            int focusCurrent = Mathf.CeilToInt(Player.Attributes.Val(AttributeType.Focus)) + compassBonus;
            _compassPulses = focusCurrent;
            UiCompassPulseController.InitialisePulses(focusMax, focusCurrent);
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
            transform.position = PathingGrid.PlayerStartPosition();
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
            if (_weaponBehaviour.Empty()) Player.BrandManager.IncreasePerfectReloadCount();
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

        //COOLDOWNS

        public void InstantReload()
        {
            _weaponBehaviour.Reload();
            StopReloading();
        }

        private IEnumerator StartReloading()
        {
            float duration = Player.EquippedWeapon.GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            _reloading = true;
            WeaponAudio.StartReload(Weapon());

            float age = 0;
            while (age < duration)
            {
                age += Time.deltaTime;
                float t = age / duration;
                UIMagazineController.UpdateReloadTime(t);
                yield return null;
            }

            float reloadFailChance = Player.Attributes.ReloadFailureChance;
            if (Random.Range(0f, 1f) >= reloadFailChance)
            {
                _weaponBehaviour.Reload();
                OnFireActions.Clear();
                ActiveSkillController.Stop();
                WeaponAudio.StopReload(Weapon());
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
            bool enemyDead = hit.HealthController.GetCurrentHealth() == 0;
            bool magazineEmpty = _weaponBehaviour.Empty();
            if (!enemyDead || !magazineEmpty) return;
            Player.BrandManager.IncreaseLastRoundKills();
            if (Player.Attributes.ReloadOnFatalShot) InstantReload();
        }

        //MISC

        public void ConsumeAmmo(int amount = -1)
        {
            _weaponBehaviour.ConsumeAmmo(amount);
        }

        public void TriggerEnemyDeathEffect()
        {
            int damage = Mathf.FloorToInt(HealthController.GetMaxHealth() * Player.Attributes.EnemyKillHealthLoss);
            TakeRawDamage(damage, Vector2.zero);
        }

        public void ReduceAdrenaline(float amount)
        {
            _adrenalineLevel.Decrement(amount);
        }

        public bool IsKeyboardBeingUsed() => _useKeyboardMovement;
    }
}