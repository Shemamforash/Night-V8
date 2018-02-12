﻿using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : CombatItem
    {
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        private const int EnemyReloadMultiplier = 4;

        public int MaxHealth;

        public Action CurrentAction;
        private readonly Cooldown _firingCooldown;
        private const float MaxAimTime = 2f;
        protected float MinimumFindCoverDistance;
        private int _wanderDirection = -1;

        public bool HasFled;
        protected bool Alerted;

        public EnemyView EnemyView;

        private const float KnockdownDuration = 5f;
        private bool _waitingForHeal;
        
        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (HealthController.GetNormalisedHealthValue() > 0.25f || _waitingForHeal) return;
                foreach (Enemy enemy in UIEnemyController.Enemies)
                {
                    Medic medic = enemy as Medic;
                    if (medic == null || medic.HasTarget()) continue;
                    CurrentAction = WaitForHeal(medic);
                    break;
                }
            });
        }

        private Action WaitForHeal(Medic medic)
        {
            SetActionText("Waiting for Medic");
            TakeCover();
            medic.RequestHeal(this);
            _waitingForHeal = true;
            return () =>
            {
                if (!medic.IsDead) return;
                _waitingForHeal = false;
                ChooseNextAction();
            };
        }

        public void ClearHealWait()
        {
            ChooseNextAction();
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            ChooseNextAction();
        }

        protected override void KnockDown()
        {
            base.KnockDown();
            CurrentAction = KnockedDown();
        }

        private Action KnockedDown()
        {
            float duration = KnockdownDuration;
            SetActionText("Knocked Down");
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                ChooseNextAction();
                IsKnockedDown = false;
            };
        }

        public virtual void ChooseNextAction()
        {
            CurrentAction = CheckForRepositioning();
            if (CurrentAction != null) return;
            CurrentAction = Aim();
        }

        protected Enemy(string name, float position) : base(name, position)
        {
            EnemyTemplate.CreateEnemyFromTemplate(this);
            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            HealthController.AddOnTakeDamage(f => { Alert(); });
            Reset();
        }

        protected void GenerateWeapon(WeaponType type)
        {
            GenerateWeapon(new List<WeaponType>{type});
        }
        
        protected void GenerateWeapon(List<WeaponType> types)
        {
            Weapon weapon = WeaponGenerator.GenerateWeapon(types, 1);
            EquipWeapon(weapon);
        }
        
        public void SetTemplate(EnemyTemplate enemyTemplate)
        {
            MovementController.SetSpeed(enemyTemplate.Speed);
            MaxHealth = enemyTemplate.Health;
        }

        public void Reset()
        {
            CharacterInventory.SetEnemyResources();
            CurrentAction = Wander;
            FacingDirection = Direction.Left;
            Alerted = false;
            Weapon?.Reload(Inventory());
        }

        public override void TakeCover()
        {
            base.TakeCover();
            SetArmour(true);
        }

        public override void LeaveCover()
        {
            base.LeaveCover();
            SetArmour(false);
        }

        private void SetArmour(bool inCover)
        {
            EnemyView?.SetArmour((int) ArmourLevel.CurrentValue(), inCover);
        }

        protected override void SetDistanceData(BasicEnemyView enemyView)
        {
            base.SetDistanceData(enemyView);
            Position.AddOnValueChange(a =>
            {
                if (EnemyView == null) return;
                if (HasFled || IsDead) return;
                _aheadOfPlayer = DistanceToPlayer > 0;
            });
        }

        private const float MeleeWarningTime = 2f;
        private const float StaggerTime = 2f;
        public const int MeleeDistance = 5;

        protected Action CheckForRepositioning(bool moveAnyway = false)
        {
            if (!InCombat()) return null;
            float absoluteDistance = Math.Abs(DistanceToPlayer);
            if (absoluteDistance <= MeleeDistance)
            {
                if (DistanceToPlayer < 0 && FacingDirection == Direction.Right
                    || DistanceToPlayer > 0 && FacingDirection == Direction.Left)
                {
                    return Melee();
                }
            }

            if (absoluteDistance < MinimumFindCoverDistance || absoluteDistance > CombatManager.VisibilityRange || moveAnyway)
            {
                float targetDistance = CalculateIdealRange();
                return MoveToTargetDistance(targetDistance);
            }

            return null;
        }


        private Action Melee()
        {
            float currentTime = MeleeWarningTime;
            SetActionText("Meleeing");
            return () =>
            {
                currentTime -= Time.deltaTime;
                if (currentTime > 0) return;
                if (Math.Abs(DistanceToPlayer) > MeleeDistance)
                {
                    CurrentAction = Stagger();
                    return;
                }

                MeleeController.StartMelee(this);
            };
        }

        private Action Stagger()
        {
            float currentTime = StaggerTime;
            SetActionText("Staggered");
            return () =>
            {
                currentTime -= Time.deltaTime;
                if (currentTime > 0) return;
                ChooseNextAction();
            };
        }

        private bool _aheadOfPlayer;

        public Action MoveToTargetDistance(float distance)
        {
            Assert.IsTrue(distance >= 0);
            Action moveForwardAction = () =>
            {
                MovementController.MoveForward();
                if (DistanceToPlayer > distance) return;
                ReachTarget();
            };
            Action moveBackwardAction = () =>
            {
                MovementController.MoveBackward();
                if (DistanceToPlayer < distance) return;
                ReachTarget();
            };

            if (_aheadOfPlayer)
            {
                SetActionText(DistanceToPlayer > distance ? "Approaching" : "Retreating");
                return DistanceToPlayer > distance ? moveForwardAction : moveBackwardAction;
            }

            SetActionText(DistanceToPlayer > distance ? "Retreating" : "Approaching");
            return DistanceToPlayer > distance ? moveBackwardAction : moveForwardAction;
        }

        private void Wander()
        {
            float distanceToTravel = Random.Range(0f, 5f);
            distanceToTravel *= _wanderDirection;
            _wanderDirection = -_wanderDirection;
            float targetPosition = Position.CurrentValue() + distanceToTravel;
            CurrentAction = MoveToTargetPosition(targetPosition);
            SetActionText("Wandering");
            CheckForPlayer();
        }

        private float CalculateIdealRange()
        {
            if (Weapon == null) return 0;
            float idealRange = Weapon.GetAttributeValue(AttributeType.Range);
            idealRange = Random.Range(0.8f * idealRange, idealRange);
            if (idealRange >= CombatManager.VisibilityRange)
            {
                idealRange = Random.Range(0.8f * CombatManager.VisibilityRange, CombatManager.VisibilityRange);
            }

            return idealRange;
        }

        private void CheckForPlayer()
        {
            if (DistanceToPlayer > _visionRange) return;
            if (FacingDirection == Direction.Right && DistanceToPlayer > 0) return;
            if (FacingDirection == Direction.Left && DistanceToPlayer < 0) return;
            CurrentAction = Suspicious;
        }

        private float _timeSinceSawPlayer = 0f;
        private const float _timeToForgetPlayer = 2f;
        
        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToPlayer >= _detectionRange.CurrentValue()) return;
            if (DistanceToPlayer >= _visionRange.CurrentValue()) CurrentAction = Wander;
            Alert();
        }

        public bool InCombat()
        {
            return !HasFled && !IsDead && Alerted;
        }

        protected void SetActionText(string text)
        {
            EnemyView?.SetActionText(text);
        }

        public override void OnHit(int damage, bool critical)
        {
            base.OnHit(damage, critical);
            if (critical)
            {
                EnemyView?.UiHitController.RegisterCritical();
            }
            else
            {
                EnemyView?.UiHitController.RegisterShot();
            }
            if (!Alerted) Alert();
        }

        public override void OnMiss()
        {
            if (!Alerted) Alert();
        }

        public override void Kill()
        {
            UIEnemyController.Remove(this);
            CombatManager.CheckCombatEnd();
            ExitCombat();
        }

        public override ViewParent CreateUi(Transform parent)
        {
            EnemyView = new EnemyView(this, parent);
            HealthController.AddOnTakeDamage(f => EnemyView?.UpdateHealth());
            HealthController.AddOnHeal(f => EnemyView?.UpdateHealth());
            EnemyView.UpdateHealth();
            ArmourLevel.AddOnValueChange(a => EnemyView?.SetArmour((int) ArmourLevel.CurrentValue(), InCover));
            EnemyView.PrimaryButton.AddOnSelectEvent(() => CombatManager.SetTarget(this));
            SetDistanceData(EnemyView);
            SetConditions();
            return EnemyView;
        }


        public virtual void Alert()
        {
            if (Alerted) return;
            Alerted = true;
            UIEnemyController.AlertAll();
            ChooseNextAction();
        }

        protected override void ReachTarget()
        {
            if (Alerted)
            {
                FacingDirection = Position.CurrentValue() > CombatManager.Player.Position.CurrentValue() ? Direction.Left : Direction.Right;
                ChooseNextAction();
            }
            else
            {
                Wander();
            }
        }

        private Action Reload()
        {
            if (Weapon.GetRemainingMagazines() == 0)
            {
                return Flee();
            }

            TakeCover();
            SetActionText("Reloading");
            float duration = Weapon.GetAttributeValue(AttributeType.ReloadSpeed) * EnemyReloadMultiplier;
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                Weapon.Reload(Inventory());
                ChooseNextAction();
            };
        }

        private Action Flee()
        {
            SetActionText("Fleeing");
            return () =>
            {
                MovementController.MoveBackward();
                CombatManager.CheckEnemyFled(this);
            };
        }

        private Action Fire()
        {
            int divider = Random.Range(3, 6);
            int noShots = (int) (Weapon.GetAttributeValue(AttributeType.Capacity) / divider);
            bool automatic = Weapon.WeaponAttributes.Automatic;
            SetActionText("Firing");
            if (!automatic)
            {
                noShots = 1;
            }

            return () =>
            {
                Shot s = FireWeapon(CombatManager.Player);
                if (s == null) return;
                s.Fire();
                EnemyView?.UiAimController.Fire();
                --noShots;
                int remainingAmmo = Weapon.GetRemainingAmmo();
                if (noShots == 0 || remainingAmmo == 0)
                {
                    UpdateAim(0);
                }

                if (remainingAmmo == 0 || !automatic)
                {
                    CurrentAction = Reload();
                }
                else if (noShots == 0)
                {
                    ChooseNextAction();
                }
            };
        }

        private void UpdateAim(float value)
        {
            EnemyView?.UiAimController.SetValue(value);
        }

        protected virtual Action Aim()
        {
            if (Weapon.Empty())
            {
                return Reload();
            }
            LeaveCover();
            Assert.IsFalse(Weapon.Empty());
            float aimTime = MaxAimTime;
            SetActionText("Aiming");
            return () =>
            {
                if (Immobilised()) return;
                float normalisedTime = Helper.Normalise(aimTime, MaxAimTime);
                UpdateAim(1 - normalisedTime);
                aimTime -= Time.deltaTime;
                if (aimTime < 0)
                {
                    CurrentAction = Fire();
                }
            };
        }

        public override void EnterCombat()
        {
            base.EnterCombat();
            Alerted = false;
            IsDead = false;
            HasFled = false;    
            Reset();
        }

        public override void ExitCombat()
        {
            base.ExitCombat();
            EnemyView = null;
            CurrentAction = null;
        }

        public override void UpdateCombat()
        {
            base.UpdateCombat();
            CurrentAction?.Invoke();
        }

        protected override void SetConditions()
        {
            base.SetConditions();
            Burn.OnConditionNonEmpty = () => EnemyView?.HealthBar.StartBurning();
            Burn.OnConditionEmpty = () => EnemyView?.HealthBar.StopBurning();
            Bleeding.OnConditionNonEmpty = () => EnemyView?.HealthBar.StartBleeding();
            Bleeding.OnConditionEmpty = () => EnemyView?.HealthBar.StopBleeding();
        }
    }
}