using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Facilitating;
using Facilitating.UIControllers;
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
        private float _damageModifier, _skillCooldownModifier;
        private Cooldown _dashCooldown;
        private bool _fired;
        private int _initialArmour;
        private Transform _pivot;
        private Quaternion _lastTargetRotation;

        private Coroutine _reloadingCoroutine;

        private Number _strengthText;

        public Characters.Player Player;

        public RageController RageController;
        public bool Retaliate;

        protected override void Move(Vector2 direction)
        {
            if (CanDash())
            {
                Dash(direction);
                _dashPressed = false;
            }
            else base.Move(direction);
        }

        //input
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (IsImmobilised) return;
            if (isHeld)
                switch (axis)
                {
                    case InputAxis.Fire:
                        if (CombatManager.EnemiesOnScreen().Count == 0) UiAreaInventoryController.SetNearestContainer(_lastNearestContainer);
                        if (!_fired || Player.Weapon.WeaponAttributes.Automatic)
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
            else
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
                }
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
            _lockedTarget = GetTarget();
        }

        private const float RotateSpeedMax = 90;
        private float _rotateSpeedCurrent;
        private const float _rotateAcceleration = 200f;

        private void Rotate(float direction)
        {
            if (CanDash())
            {
                Spin(direction);
                _dashPressed = false;
                return;
            }

            if (_lockedTarget != null) return;
            _rotateSpeedCurrent += _rotateAcceleration * Time.deltaTime;
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

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.Fire:
                    _fired = false;
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

        private void CheckForTarget()
        {
//            if (_currentTarget != null && !_currentTarget.OnScreen())
//            {
//                CombatManager.Select(1);
//            }
//
//            if (_currentTarget == null)
//            {
//                SetTarget(CombatManager.NearestEnemy());
//            }
        }

        private void CheckForEnemiesOnScreen()
        {
            PlayerUi.Instance().SetAlpha(CombatManager.AllEnemiesDead() ? 0 : 1);
        }

        public override void Update()
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.x = transform.position.x;
            cameraPosition.y = transform.position.y;
            Camera.main.transform.position = cameraPosition;
            base.Update();
            FollowTarget();
            CheckForTarget();
            TransitionOffScreen();
            CheckForContainersNearby();
            CheckForEnemiesOnScreen();
        }

        private const float MaxShowInventoryDistance = 0.5f;
        private ContainerController _lastNearestContainer;

        private void CheckForContainersNearby()
        {
            ContainerController nearestContainer = null;
            float nearestContainerDistance = MaxShowInventoryDistance;
            ContainerController.Containers.ForEach(c =>
            {
                float distance = Vector2.Distance(c.transform.position, CombatManager.Player().transform.position);
                if (distance > nearestContainerDistance) return;
                nearestContainerDistance = distance;
                nearestContainer = c.ContainerController;
            });
            _lastNearestContainer = nearestContainer;
        }

        public void Initialise()
        {
            _pivot = Helper.FindChildWithName<Transform>(gameObject, "Pivot");
            InputHandler.SetCurrentListener(this);

            Player = CharacterManager.SelectedCharacter;
            ArmourController = Player.ArmourController;
            _damageModifier = Player.CalculateDamageModifier();
            _skillCooldownModifier = Player.CalculateSkillCooldownModifier();
            _initialArmour = Player.ArmourController.GetProtectionLevel();

//            _playerUi._playerName.text = player.Name;
            Speed = Player.CalculateSpeed();

            _dashCooldown = CombatManager.CreateCooldown();
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

            HealthController.SetInitialHealth(Player.CalculateCombatHealth(), this);
            HealthController.AddOnHeal(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue()));

            HeartBeatController.SetHealth(HealthController.GetNormalisedHealthValue());

            SkillBar.BindSkills(Player);
            UIMagazineController.SetWeapon(Weapon());
        }

        protected override void Dash(Vector2 direction)
        {
            base.Dash(direction);
            _dashCooldown.Start();
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished() && _dashPressed;
        }

        public void TryRetaliate(EnemyBehaviour origin)
        {
            if (Retaliate) FireWeapon();
        }

        public override void Knockback(Vector3 source, float force = 10f)
        {
            base.Knockback(source, force);
            StopReloading();
            UpdateMagazineUi();
        }

        public void SetTarget(EnemyBehaviour e)
        {
            if (_lockedTarget != null) return;
            if (e != null && !Helper.IsObjectInCameraView(e.gameObject)) return;
            _currentTarget = e;
            EnemyUi.Instance().SetSelectedEnemy(e);
        }

        public override Weapon Weapon()
        {
            return Player.Weapon;
        }

        public void ExitCombat()
        {
            StopReloading();
            _fired = false;
        }

        //RELOADING
        private void Reload()
        {
            if (_reloadingCoroutine != null) return;
            if (Player.Weapon.FullyLoaded()) return;
            _reloadingCoroutine = StartCoroutine(StartReloading());
        }

        private void StopReloading()
        {
            if (_reloadingCoroutine != null) StopCoroutine(_reloadingCoroutine);
            _reloadingCoroutine = null;
            Immobilised(false);
            UpdateMagazineUi();
        }

        //COOLDOWNS

        private IEnumerator StartReloading()
        {
            Immobilised(true);
            float duration = Player.Weapon.GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            _fired = false;
            OnFireAction = null;
            Retaliate = false;

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

            Player.Weapon.Reload(Player.Inventory());
            OnReloadAction?.Invoke();
            StopReloading();
        }


        //FIRING
        public void FireWeapon()
        {
//            if (GetTarget() == null) return;
            if (Weapon().Empty()) return;
            List<Shot> shots = Weapon().Fire(this);
            if (shots.Count == 0) return;
            shots.ForEach(shot =>
            {
                shot.SetDamageModifier(_damageModifier);
                OnFireAction?.Invoke(shot);
                shot.Fire();
            });
            _fired = true;
            UpdateMagazineUi();
        }

        private bool ShowMuzzleFlash;
        private SpriteRenderer _muzzleFlash;

        private void UpdateMuzzleFlash()
        {
            if (_muzzleFlash == null) _muzzleFlash = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Muzzle Flash");
            if (ShowMuzzleFlash)
            {
                _muzzleFlash.color = new Color(1,1,1, Random.Range(0.2f, 0.8f));
                ShowMuzzleFlash = false;
            }
            else
            {
                _muzzleFlash.color = UiAppearanceController.InvisibleColour;
            }
        }

        //MISC

        public void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (Player.Weapon.Empty())
                magazineMessage = "RELOAD";
            if (magazineMessage == "")
            {
                UIMagazineController.UpdateMagazine();
            }
            else
            {
                UIMagazineController.EmptyMagazine();
            }
        }

        public override CharacterCombat GetTarget()
        {
            return _currentTarget;
        }
    }
}