using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EZCameraShake;
using Facilitating;
using Facilitating.UIControllers;
using Fastlights;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
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
using Random = UnityEngine.Random;

namespace Game.Combat.Player
{
    public class PlayerCombat : CharacterCombat, IInputListener, ICombatEvent
    {
        private bool _reloading;
        private float _dryFireTimer;
        private float _adrenalineRecoveryRate;
        private const float DryFireTimerMax = 0.3f;
        private readonly Number _adrenalineLevel = new Number(0, 0, 8);
        private Coroutine _dashCooldown;
        private Quaternion _lastTargetRotation;
        private int _compassPulses;

        public Characters.Player Player;
        public FastLight _playerLight;

        public static PlayerCombat Instance;

        public float MuzzleFlashOpacity;

        private const float RotateSpeedMax = 150f;
        private float _rotateSpeedCurrent;
        private const float RotateAcceleration = 400f;
        private bool _recovered;
        public Action<Shot> OnFireAction;
        public List<Action> UpdateSkillActions = new List<Action>();
        public BaseWeaponBehaviour _weaponBehaviour;
        private FastLight _muzzleFlash;
        private DeathReason _currentDeathReason;
        private bool _useKeyboardMovement = true;
        private Vector2? _lastMousePosition;
        private Camera _mainCamera;
        private bool _swivelling;
        private float _swivelAmount;
        public static bool Alive;
        private string _controlText;
        private float _dashCooldownTime;

        public static Vector3 Position()
        {
            return Instance.transform.position;
        }

        private void OnDestroy()
        {
            Alive = false;
            Instance = null;
        }

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
            MovementController.Move(direction);
        }

        protected override void Awake()
        {
            IsPlayer = true;
            base.Awake();
            BurnDamagePercent = 0.025f;
            Instance = this;
            _mainCamera = Camera.main;
        }

        public void Start()
        {
            ControlTypeChangeListener controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
            controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
        }

        private void UpdateText()
        {
            _controlText = InputHandler.GetBindingForKey(InputAxis.TakeItem);
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
            if (gameObject == null) return;
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
                        _rotatingWithKeyboard = true;
                        Rotate(direction);
                        break;
                    case InputAxis.Reload:
                        Reload();
                        break;
                }
            }
            else
            {
                switch (axis)
                {
                    case InputAxis.SkillOne:
                        SkillBar.Instance().ActivateSkill(0);
                        break;
                    case InputAxis.SkillTwo:
                        SkillBar.Instance().ActivateSkill(1);
                        break;
                    case InputAxis.SkillThree:
                        SkillBar.Instance().ActivateSkill(2);
                        break;
                    case InputAxis.SkillFour:
                        SkillBar.Instance().ActivateSkill(3);
                        break;
                    case InputAxis.Sprint:
                        TryDash();
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
                    case InputAxis.Swivel:
                        StartSwivelling();
                        break;
                }
            }
        }

        private void StartSwivelling()
        {
            if (_swivelling) return;
            _swivelling = true;
            transform.rotation = Quaternion.Euler(0, 0, _mainCamera.transform.rotation.z);
        }

        private void TryDash()
        {
            if (!CanDash()) return;
            MovementController.Dash();
            _dashCooldown = StartCoroutine(Dash());
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
                case InputAxis.Fire:
                    _weaponBehaviour.StopFiring();
                    break;
                case InputAxis.SwitchTab:
                    _rotateSpeedCurrent = 0f;
                    _rotatingWithKeyboard = false;
                    break;
                case InputAxis.Swivel:
                    _swivelling = false;
                    break;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        private void Rotate(float direction)
        {
            _useKeyboardMovement = true;
            direction = Mathf.Clamp(direction, -1, 1);
            _rotateSpeedCurrent += RotateAcceleration * Time.deltaTime * Mathf.Abs(direction);
            if (_rotateSpeedCurrent > RotateSpeedMax) _rotateSpeedCurrent = RotateSpeedMax;
            transform.Rotate(Vector3.forward, _rotateSpeedCurrent * Time.deltaTime * (-direction).Polarity());
        }

        private void UpdateRotation()
        {
            if (_rotatingWithKeyboard) return;
            Vector2 mouseScreenPosition = Input.mousePosition;
            bool ignoreMouseRotation = _lastMousePosition == null || _useKeyboardMovement && mouseScreenPosition == _lastMousePosition.Value || _swivelling;
            _lastMousePosition = mouseScreenPosition;
            if (ignoreMouseRotation) return;
            _useKeyboardMovement = false;
            Vector2 mousePosition = Helper.MouseToWorldCoordinates();
            float rotation = AdvancedMaths.AngleFromUp(transform.position, mousePosition);
            transform.rotation = Quaternion.Euler(0, 0, rotation);
        }

        public float InRange()
        {
            if (!CharacterManager.CurrentRegion().IsDynamic() && CharacterManager.CurrentRegion().GetRegionType() != RegionType.Temple) return -1;
            return CurrentCell().OutOfRange || CurrentCell().IsEdgeCell ? 1 : -1;
        }

        public string GetEventText()
        {
            return "Leave region [" + _controlText + "]";
        }

        public override int Burn()
        {
            int burnDamage = base.Burn();
            if (burnDamage == -1) return -1;
            Player.BrandManager.IncreaseBurnCount(burnDamage);
            _currentDeathReason = DeathReason.Fire;
            return burnDamage;
        }

        public override bool Void()
        {
            if (!base.Void()) return false;
            _currentDeathReason = DeathReason.Void;
            Instance.Player.BrandManager.IncreaseVoidCount();
            return true;
        }

        public override void Decay()
        {
            base.Decay();
            if (ArmourController.CanAbsorbDamage()) Instance.Player.BrandManager.IncreaseDecayCount();
        }

        public void Activate()
        {
            CombatManager.Instance().ExitCombat();
        }

        public void ExitCombat()
        {
            Alive = false;
            InputHandler.UnregisterInputListener(this);
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        public override void Kill()
        {
            if (!Alive) return;
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

            if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Rite)
            {
                ShrineBehaviour.ActiveShrine.Fail();
                return;
            }

            InputHandler.SetCurrentListener(null);
            InputHandler.UnregisterInputListener(this);
            WeaponAudio.Destroy();
            Alive = false;
            bool isWanderer = Player.CharacterTemplate.CharacterClass == CharacterClass.Wanderer;
            if (!isWanderer) CombatManager.Instance().ExitCombat();
            Player.Kill(_currentDeathReason);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            UpdateSkillActions.ForEach(a => a());
            UpdateMuzzleFlash();
            TrySwivel();
            UpdateRotation();
            CheckBrandUnlock();
        }

        private void CheckBrandUnlock()
        {
            if (!RiteStarter.Available()) return;
            Region currentRegion = CharacterManager.CurrentRegion();
            if (currentRegion == null) return;
            if (!currentRegion.IsDynamic()) return;
            Brand unlocked = Player.BrandManager.GetActiveBrands().FirstOrDefault(b => b != null && b.Ready());
            if (unlocked == null) return;
            RiteStarter.Generate(unlocked);
        }

        private void TrySwivel()
        {
            if (!_swivelling) return;
            if (_lastMousePosition == null) return;
            _swivelAmount = _lastMousePosition.Value.x - Input.mousePosition.x;
            if (_swivelAmount == 0) return;
            _swivelAmount *= 0.2f;
            float maxAngle = 10;
            _swivelAmount = Mathf.Clamp(_swivelAmount, -maxAngle, maxAngle);
            transform.Rotate(Vector3.forward, _swivelAmount);
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
            shot.Attributes().SetDamageModifier(WorldState.GetEnemyDamageModifier());
            base.TakeShotDamage(shot);
            UpdateSkillActions.Clear();
        }

        public override void TakeExplosionDamage(int damage, Vector2 direction, float radius)
        {
            _currentDeathReason = DeathReason.Standard;
            base.TakeExplosionDamage(damage, direction, radius);
            Shake(damage * 10);
        }

        protected override void TakeDamage(int damage, Vector2 direction)
        {
            if (damage < 1) damage = 1;
            Player.BrandManager.IncreaseDamageTaken(damage);
            if (ArmourController.CanAbsorbDamage()) WeaponAudio.PlayShieldHit();
            else WeaponAudio.PlayBodyHit();
            base.TakeDamage(damage, direction);
            TryExplode();
            Player.Attributes.CalculateNewLife(HealthController.GetCurrentHealth());
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

        public void EquipWeapon()
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
            SkillBar.UpdateSkills();
            _adrenalineRecoveryRate = Player.Attributes.CalculateAdrenalineRecoveryRate();
            _dashCooldownTime = Player.Attributes.CalculateDashCooldown();
        }

        private void EquipArmour()
        {
            ArmourController = Player.ArmourController;
            ArmourController.Reset();
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
            float duration = _dashCooldownTime;
            while (duration > 0f)
            {
                duration -= Time.deltaTime;
                if (duration < 0f) duration = 0f;
                float normalisedTime = 1f - duration / _dashCooldownTime;
                RageBarController.UpdateDashTimer(normalisedTime);
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
            Alive = true;
            _muzzleFlash = GameObject.Find("Muzzle Flash").GetComponent<FastLight>();
            Player = CharacterManager.SelectedCharacter;
            RecalculateHealth();
            EquipWeapon();
            EquipArmour();
            ResetCompass();
            _adrenalineLevel.SetCurrentValue(0f);

            _playerLight = GameObject.Find("Player Light").GetComponent<FastLight>();
            _playerLight.Radius = CombatManager.Instance().VisibilityRange();

            SkillBar.UpdateSkills();
            transform.position = WorldGrid.PlayerStartPosition();
            float zRot = AdvancedMaths.AngleFromUp(transform.position, Vector2.zero);
            transform.rotation = Quaternion.Euler(0f, 0f, zRot);
        }

        public override float GetRecoilModifier()
        {
            return _weaponBehaviour is AccuracyGainer ? 1 - base.GetRecoilModifier() : base.GetRecoilModifier();
        }

        public void Shake(float dps)
        {
            float magnitude = dps / 500f;
            if (magnitude > 1) magnitude = 1f;
            CameraShaker.Instance.ShakeOnce(magnitude, 10, 0.2f, 0.2f);
        }

        private bool CanDash()
        {
            return _dashCooldown == null;
        }

        public override Weapon Weapon() => Player.EquippedWeapon;


        private float _reloadDuration, _currentReloadTime;
        private bool _rotatingWithKeyboard;

        //RELOADING
        private void Reload()
        {
            if (!_weaponBehaviour.CanReload()) return;
            if (!_reloading) StartReloading();
            UpdateReloading();
        }

        private void StartReloading()
        {
            _reloading = true;
            _reloadDuration = Player.EquippedWeapon.GetAttributeValue(AttributeType.ReloadSpeed);
            _currentReloadTime = 0f;
            WeaponAudio.StartReload(Weapon());
            OnFireAction = null;
            UIMagazineController.EmptyMagazine();
            ReloadController.Instance().Show();
        }

        private void UpdateReloading()
        {
            _currentReloadTime += Time.deltaTime;
            float normalisedTime = _currentReloadTime / _reloadDuration;
            if (_currentReloadTime > _reloadDuration)
            {
                StopReloading();
                return;
            }

            ReloadController.Instance().SetProgress(normalisedTime);
            UIMagazineController.UpdateReloadTime(normalisedTime);
            _dryFireTimer = 0f;
        }

        private void StopReloading()
        {
            ReloadController.Instance().Complete();
            _weaponBehaviour.Reload();
            ActiveSkillController.Stop();
            WeaponAudio.StopReload(Weapon());
            UIMagazineController.UpdateMagazineUi();
            _reloading = false;
        }

        //COOLDOWNS

        private void InstantReload()
        {
            StopReloading();
        }

        public override void ApplyShotEffects(Shot s)
        {
            if (OnFireAction == null) return;
            OnFireAction(s);
            WeaponAudio.PlayPassiveSkill();
        }

        //FIRING
        private void FireWeapon()
        {
            if (_reloading) return;
            if (_weaponBehaviour.Empty())
            {
                if (Player.Attributes.ReloadOnEmptyMag) Reload();
                else if (!_reloading) StartReloading();
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

        public bool IsKeyboardBeingUsed() => _useKeyboardMovement || (_swivelling && _swivelAmount != 0);
    }
}