using System;
using System.Net;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : CombatItem
    {
        public readonly CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        public readonly CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);

        public readonly int MaxHealth;

        protected Action CurrentAction;
        private Cooldown _aimCooldown;
        private Cooldown _fireCooldown;
        private readonly Cooldown _firingCooldown;
        private float _aimTime = 2f;
        protected float PreferredCoverDistance;
        protected float MinimumFindCoverDistance;
        private int _wanderDirection = -1;
        private Cooldown _wanderCooldown;

        protected bool AlertOthers;
        protected bool ShowMovementText = true;
        public bool HasFled, IsDead;
        protected bool Alerted;
        public bool WaitingForHeal;
        
        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (!(HealthController.GetNormalisedHealthValue() <= 0.5f) || WaitingForHeal) return;
                foreach (Enemy enemy in CombatManager.GetEnemies())
                {
                    Medic medic = enemy as Medic;
                    medic?.RequestHeal(this);
                    SetActionText("Waiting for Healing");
                    TakeCover();
                    WaitingForHeal = true;
                }
            });
        }

        public EnemyView EnemyView()
        {
            return (EnemyView)EnemyUi;
        }
        
        private void SetFireCooldown()
        {
            _fireCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _fireCooldown.SetStartAction(() => SetActionText("Firing"));
            _fireCooldown.SetDuringAction(f => TryFire());
        }

        private void SetAimCooldown()
        {
            _aimCooldown = new Cooldown(CombatManager.CombatCooldowns, _aimTime);
            _aimCooldown.SetStartAction(() => SetActionText("Aiming"));
            _aimCooldown.SetDuringAction(f =>
            {
                float normalisedTime = Helper.Normalise(f, _aimTime);
                EnemyView().UiAimController.SetValue(1 - normalisedTime);
            });
            _aimCooldown.SetEndAction(() =>
            {
                _fireCooldown.Duration = Random.Range(1, 3);
                _fireCooldown.Start();
                EnemyView().UiAimController.SetValue(0);
            });
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            WaitingForHeal = false;
        }

        protected Enemy(string name, int enemyHp, int speed, int distance = 0) : base(name, speed, distance)
        {
            MaxHealth = enemyHp;
            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CharacterInventory.SetEnemyResources();

            if (distance == 0) Distance.SetCurrentValue(Random.Range(25, 50));

            SetDistanceData();
            SetFireCooldown();
            SetAimCooldown();
            
            ReloadingCooldown.SetStartAction(() => SetActionText("Reloading"));
//            CoverCooldown.SetStartAction(() => SetActionText("Taking Cover"));
            SetWanderCooldown();
            CurrentAction = Wander;
            HealthController.AddOnTakeDamage(f => { TryAlert(); });
            
            TakeCoverAction = () => EnemyView().SetArmour((int) ArmourLevel.CurrentValue(), true);
            LeaveCoverAction = () => EnemyView().SetArmour((int) ArmourLevel.CurrentValue(), true);
        }

        protected override Shot FireWeapon(Character target)
        {
            Shot s = base.FireWeapon(target);
            if (s != null) EnemyView().UiAimController.Fire();
            return s;
        }

        private void SetDistanceData()
        {
            Distance.AddOnValueChange(a =>
            {
                if (EnemyView() == null) return;
                if (HasFled || IsDead) return;
                EnemyView().RangeText.text = Distance.GetThresholdName();
                if (a.CurrentValue() <= MaxDistance) return;
                HasFled = true;
            });
        }

        protected override void MoveToTargetDistance()
        {
            if (Distance.CurrentValue() > TargetDistance)
            {
                if (ShowMovementText) SetActionText("Approaching");
            }
            else
            {
                if (ShowMovementText) SetActionText("Retreating");
            }
        }
        
        public bool InCombat()
        {
            return !HasFled && !IsDead;
        }

        public void AddVisionModifier(float amount)
        {
//            VisionRange.AddModifier(amount);
//            DetectionRange.AddModifier(amount);
        }

        public void RemoveVisionModifier(float amount)
        {
//            VisionRange.RemoveModifier(amount);
//            DetectionRange.RemoveModifier(amount);
        }

        public void TryAlert()
        {
            if (Alerted) return;
            Alert();
        }

        protected void SetActionText(string action)
        {
            if (EnemyView() == null) return;
            EnemyView().ActionText.text = action;
        }

        private void UpdateDetection()
        {
            if (Alerted) return;
            if (Distance < DetectionRange && !Alerted)
            {
                EnemyView().SetDetected();
                TryAlert();
                return;
            }
            if (Distance < VisionRange)
            {
                EnemyView().SetAlert();
                return;
            }
            EnemyView().SetUnaware();
        }

        public override void OnMiss()
        {
//            EnemyBehaviour.TakeFire();
        }

        public override void Kill()
        {
            IsDead = true;
            EnemyView()?.MarkDead();
            CombatManager.CheckCombatEnd();
        }

        public override ViewParent CreateUi(Transform parent)
        {
            EnemyUi = new EnemyView(this, parent);
            HealthController.AddOnTakeDamage(f => EnemyView().SetHealth(HealthController));
            HealthController.AddOnHeal(f => EnemyView().SetHealth(HealthController));
            EnemyView().SetHealth(HealthController);
            ArmourLevel.AddOnValueChange(a => EnemyView().SetArmour((int) ArmourLevel.CurrentValue(), InCover));
            EnemyView().PrimaryButton.AddOnSelectEvent(() => CombatManager.SetTarget(this));
            SetConditions();
            return EnemyView();
        }

        public string EnemyType()
        {
            return "Default Enemy";
        }

        protected void SetAimTime(float aimTime)
        {
            _aimTime = aimTime;
            _aimCooldown.Duration = _aimTime;
        }

        protected virtual void Alert()
        {
            Assert.IsFalse(Alerted);
            Alerted = true;
            _wanderCooldown.Cancel();
            if (AlertOthers)
            {
                CombatManager.GetEnemies().ForEach(e => { e.TryAlert(); });
            }
        }

        private void Wander()
        {
            if (_wanderCooldown.Running()) return;
            _wanderCooldown.Start();
        }
        
        private void SetWanderCooldown()
        {
            _wanderCooldown = CombatManager.CombatCooldowns.CreateCooldown(2f);
            _wanderCooldown.SetStartAction(() => SetActionText("Wandering"));
            _wanderCooldown.SetDuringAction(f =>
            {
                MovementController.Move(-_wanderDirection);
            });
            _wanderCooldown.SetEndAction(() =>
            {
                _wanderCooldown.Duration = Random.Range(1f, 3f);
                _wanderDirection = -_wanderDirection;
            });
            CurrentAction = Wander;
        }

        protected void FindBetterRange()
        {
            if (Moving()) return;
            TargetDistance = Random.Range(PreferredCoverDistance * 0.9f, PreferredCoverDistance * 1.1f);
            CurrentAction = MoveToTargetDistance;
        }

        protected override void ReachTarget()
        {
            base.ReachTarget();
            CurrentAction = AnticipatePlayer;
        }

        protected void AnticipatePlayer()
        {
            if (Distance < MinimumFindCoverDistance)
            {
                FindBetterRange();
                return;
            }
            CurrentAction = AimAndFire;
        }

        private void TryFire()
        {
            if (_aimCooldown.Running()) return;
            if (NeedsCocking() || Weapon().Empty())
            {
                TakeCover();
                TryReload();
                return;
            }
            LeaveCover();
            FireWeapon(CombatManager.Player());
        }

        private void AimAndFire()
        {
            Assert.IsFalse(Moving());
            if (Immobilised()) return;
            if (Weapon().Empty())
            {
                TakeCover();
                TryReload();
                return;
            }
            if (_aimCooldown.Running() || _fireCooldown.Running()) return;
            _aimCooldown.Start();
        }

        public override void Update()
        {
            if (!InCombat()) return;
            base.Update();
            UpdateDetection();
            if (IsDead || WaitingForHeal) return;
            CurrentAction?.Invoke();
            PrintUpdate();
        }

        protected virtual void PrintUpdate()
        {
            
        }

        protected override void SetConditions()
        {
            base.SetConditions();
            Burning.OnConditionNonEmpty = EnemyView().StartBurning;
            Burning.OnConditionEmpty = EnemyView().StopBurning;
            Bleeding.OnConditionNonEmpty = EnemyView().StartBleeding;
            Bleeding.OnConditionEmpty = EnemyView().StopBleeding;
            Sickening.OnConditionNonEmpty = () => EnemyView().UpdateSickness(((Sickness)Sickening).GetNormalisedValue());
            Burning.OnConditionEmpty = () => EnemyView().UpdateSickness(0);
        }
    }
}