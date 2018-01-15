using System;
using Game.Characters;
using Game.Combat.Enemies.EnemyTypes;
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
    public class Enemy : Character
    {
        public const int ImmediateDistance = 1, CloseDistance = 10, MidDistance = 50, FarDistance = 100, MaxDistance = 150;
        public readonly CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        public readonly CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);

        private EnemyView _enemyView;
        public readonly Number Distance = new Number(0, 0, 150);
        public readonly int MaxHealth;
        public int Speed;

        protected float TargetDistance;
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
                _enemyView.UiAimController.SetValue(1 - normalisedTime);
            });
            _aimCooldown.SetEndAction(() =>
            {
                _fireCooldown.Duration = Random.Range(1, 3);
                _fireCooldown.Start();
                _enemyView.UiAimController.SetValue(0);
            });
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            WaitingForHeal = false;
        }

        protected Enemy(string name, int enemyHp, int distance = 0) : base(name)
        {
            MaxHealth = enemyHp;
            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CharacterInventory.SetEnemyResources();

            if (distance == 0) distance = Random.Range(25, 50);
            Distance.SetCurrentValue(distance);

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
            if (s != null) _enemyView.UiAimController.Fire();
            return s;
        }

        protected override float GetSpeedModifier()
        {
            return Speed * Time.deltaTime;
        }

        private void SetDistanceData()
        {
            Distance.AddThreshold(ImmediateDistance, "Immediate");
            Distance.AddThreshold(CloseDistance, "Close");
            Distance.AddThreshold(MidDistance, "Medium");
            Distance.AddThreshold(FarDistance, "Far");
            Distance.AddThreshold(MaxDistance, "Out of Range");
            Distance.AddOnValueChange(a =>
            {
                if (_enemyView == null) return;
                if (HasFled || IsDead) return;
                float distance = Helper.Round(Distance.CurrentValue());
                string distanceText = distance.ToString() + "m";
                _enemyView.DistanceText.text = distanceText;
                _enemyView.RangeText.text = Distance.GetThresholdName();
                float normalisedDistance = Helper.Normalise(distance, MaxDistance);
                float alpha = 1f - normalisedDistance;
                alpha *= alpha;
                alpha = Mathf.Clamp(alpha, 0.2f, 1f);
                _enemyView.SetAlpha(alpha);
                if (a.CurrentValue() <= MaxDistance) return;
                HasFled = true;
            });
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
            _enemyView.ActionText.text = action;
        }

        private void UpdateDetection()
        {
            if (Alerted) return;
            if (Distance < DetectionRange && !Alerted)
            {
                _enemyView.SetDetected();
                TryAlert();
                return;
            }
            if (Distance < VisionRange)
            {
                _enemyView.SetAlert();
                return;
            }
            _enemyView.SetUnaware();
        }

        public EnemyView EnemyView()
        {
            return _enemyView;
        }

        public override void OnMiss()
        {
//            EnemyBehaviour.TakeFire();
        }

        public override void Kill()
        {
            IsDead = true;
            _enemyView.MarkDead();
            CombatManager.CheckCombatEnd();
        }

        public override ViewParent CreateUi(Transform parent)
        {
            _enemyView = new EnemyView(this, parent);
            HealthController.AddOnTakeDamage(f => _enemyView.SetHealth(HealthController));
            HealthController.AddOnHeal(f => _enemyView.SetHealth(HealthController));
            _enemyView.SetHealth(HealthController);
            ArmourLevel.AddOnValueChange(a => _enemyView.SetArmour((int) ArmourLevel.CurrentValue(), InCover));
            return _enemyView;
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
                if (_wanderDirection == -1) MoveForward();
                else MoveBackward();
            });
            _wanderCooldown.SetEndAction(() =>
            {
                _wanderCooldown.Duration = Random.Range(1f, 3f);
                _wanderDirection = -_wanderDirection;
            });
            CurrentAction = Wander;
        }

        protected bool Moving()
        {
            return TargetDistance > 0;
        }

        protected void FindBetterRange()
        {
            if (Moving()) return;
            TargetDistance = Random.Range(PreferredCoverDistance * 0.9f, PreferredCoverDistance * 1.1f);
            CurrentAction = MoveToTargetDistance;
        }

        protected void MoveToTargetDistance()
        {
            float currentDistance = Distance.CurrentValue();
            if (currentDistance > TargetDistance)
            {
                MoveForward();
                if (ShowMovementText) SetActionText("Approaching");
                float newDistance = Distance.CurrentValue();
                if (!(newDistance <= TargetDistance)) return;
                ReachTarget();
            }
            else
            {
                MoveBackward();
                if (ShowMovementText) SetActionText("Retreating");
                float newDistance = Distance.CurrentValue();
                if (!(newDistance >= TargetDistance)) return;
                ReachTarget();
            }
        }

        protected virtual void ReachTarget()
        {
            Distance.SetCurrentValue(TargetDistance);
            TargetDistance = -1;
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

        public void UpdateBehaviour()
        {
            if (!InCombat()) return;
            UpdateDetection();
            if (IsDead || WaitingForHeal) return;
            CurrentAction?.Invoke();
            PrintUpdate();
        }

        protected virtual void PrintUpdate()
        {
            
        }
    }
}