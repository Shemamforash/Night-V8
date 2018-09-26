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
        private EnemyBehaviour _currentTarget;
        private float _skillCooldownModifier;
        private readonly Number _adrenalineLevel = new Number(0, 0, 8);
        private Coroutine _dashCooldown;
        private Quaternion _lastTargetRotation;
        private int _compassPulses;

        private Coroutine _reloadingCoroutine;

        private Number _strengthText;

        public Characters.Player Player;
        public FastLight _playerLight;

        public static PlayerCombat Instance;

        public float MuzzleFlashOpacity;

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
            _adrenalineLevel.Decrement(amount);
            RageBarController.SetRageBarFill(_adrenalineLevel.Normalised());
            return true;
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

        public float InRange()
        {
            return CurrentCell().OutOfRange ? 1 : -1;
        }

        public string GetEventText()
        {
            return "Leave region [T}";
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

        public void UpdateAdrenaline(int damageDealt)
        {
            _adrenalineLevel.Increment(damageDealt / 150f * _adrenalineRecoveryRate);
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
            Shake(damage * 20);
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
            ArmourBarController().TakeDamage(ArmourController);
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

        public void Initialise()
        {
            InputHandler.SetCurrentListener(this);

            _muzzleFlash = GameObject.Find("Muzzle Flash").GetComponent<FastLight>();
            Player = CharacterManager.SelectedCharacter;
            Player.Inventory().Move(WeaponGenerator.GenerateWeapon(ItemQuality.Glowing), 1);
            for (int i = 0; i < 50; ++i)
            {
                Player.Inventory().Move(Inscription.Generate(10), 1);
            }

            Player.Inventory().Move(ArmourPlate.Create(ItemQuality.Radiant), 1);
            Player.Inventory().IncrementResource("Essence", 50);
            HealthController.SetInitialHealth(Player.Attributes.CalculateInitialHealth(), this, Player.Attributes.CalculateMaxHealth());
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

        public void Knockback(Vector3 source, float force = 10f)
        {
            MovementController.KnockBack(source, force);
            StopReloading();
        }

        public void SetTarget(EnemyBehaviour e)
        {
            Debug.Log(e + " " + _lockedTarget);
            if (_lockedTarget != null) return;
            if (e != null) return;
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
            CombatManager.SetHasFiredShot();
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

        public void OnShotConnects(ITakeDamageInterface hit)
        {
            if (!Player.Attributes.ReloadOnLastRound || !_weaponBehaviour.Empty() || !hit.IsDead()) return;
            InstantReload();
        }

        //MISC

        public override CharacterCombat GetTarget() => _currentTarget;

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