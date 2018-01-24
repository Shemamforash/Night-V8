using System;
using System.Net;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Misc;
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
        private readonly Cooldown _firingCooldown;
        private const float MaxAimTime = 2f;
        protected float MinimumFindCoverDistance;
        protected float DefaultDistance = 20f;
        private int _wanderDirection = -1;

        public bool HasFled, IsDead;
        protected bool Alerted;
        public bool WaitingForHeal;

        public EnemyView EnemyView;

        protected Cooldown KnockdownCooldown;
        private const float KnockdownDuration = 3f;

        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (!(HealthController.GetNormalisedHealthValue() <= 0.5f) || WaitingForHeal) return;
                foreach (Enemy enemy in CombatManager.GetEnemies())
                {
                    Medic medic = enemy as Medic;
                    if (medic == null) continue;
                    medic.RequestHeal(this);
                    SetActionText("Waiting for Healing");
                    TakeCover();
                    WaitingForHeal = true;
                }
            });
        }

        public void ClearHealWait()
        {
            WaitingForHeal = false;
        }

        private void SetKnockdownCooldown()
        {
            KnockdownCooldown = CombatManager.CombatCooldowns.CreateCooldown(KnockdownDuration);
            KnockdownCooldown.SetEndAction(() => KnockedDown = false);
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            WaitingForHeal = false;
        }

        public override void KnockDown()
        {
            if (!KnockedDown) return;
            KnockdownCooldown.Start();
            base.KnockDown();
        }

        protected Enemy(string name, int enemyHp, int speed, float position) : base(name, speed, position)
        {
            MaxHealth = enemyHp;
            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CharacterInventory.SetEnemyResources();

            SetKnockdownCooldown();

            ReloadingCooldown.SetStartAction(() => SetActionText("Reloading"));
//            CoverCooldown.SetStartAction(() => SetActionText("Taking Cover"));
            CurrentAction = Wander;
            HealthController.AddOnTakeDamage(f => { Alert(); });

            TakeCoverAction = () => EnemyView.SetArmour((int) ArmourLevel.CurrentValue(), true);
            LeaveCoverAction = () => EnemyView.SetArmour((int) ArmourLevel.CurrentValue(), true);
        }

        protected override void SetDistanceData(BasicEnemyView enemyView)
        {
            base.SetDistanceData(enemyView);
            Position.AddOnValueChange(a =>
            {
                if (EnemyView == null) return;
                if (HasFled || IsDead) return;
                if (DistanceToPlayer <= MaxDistance)
                {
                    CheckForRepositioning();
                    return;
                }

                HasFled = true;
            });
        }

        protected Action MoveToTargetDistance(float distance)
        {
            SetActionText(DistanceToPlayer > distance ? "Approaching" : "Retreating");

            if (DistanceToPlayer > distance)
            {
                return () =>
                {
                    MovementController.MoveForward();
                    if (DistanceToPlayer > distance) return;
                    ReachTarget();
                };
            }

            return () =>
            {
                MovementController.MoveBackward();
                if (DistanceToPlayer < distance) return;
                ReachTarget();
            };
        }

        private void Wander()
        {
            float distanceToTravel = Random.Range(0f, 5f);
            distanceToTravel *= _wanderDirection;
            _wanderDirection = -_wanderDirection;
            float targetPosition = Position.CurrentValue() + distanceToTravel;
            CurrentAction = MoveToTargetPosition(targetPosition);
            SetActionText("Wandering");
        }

        public void CheckForRepositioning()
        {
            if (DistanceToPlayer < MinimumFindCoverDistance || DistanceToPlayer > CombatManager.VisibilityRange)
            {
                CurrentAction = MoveToTargetDistance(DefaultDistance);
            }
        }

        private void CheckAlertLevel()
        {
            if (Alerted) return;
            if (DistanceToPlayer < DetectionRange)
            {
                Alert();
                return;
            }

            if (DistanceToPlayer < VisionRange)
            {
                CurrentAction = Suspicious;
            }
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
        }

        public bool InCombat()
        {
            return !HasFled && !IsDead;
        }

        protected void SetActionText(string action)
        {
            if (EnemyView == null) return;
            EnemyView.ActionText.text = action;
        }

        public override void OnMiss()
        {
//            EnemyBehaviour.TakeFire();
        }

        public override void Kill()
        {
            IsDead = true;
            EnemyView?.MarkDead();
            CombatManager.CheckCombatEnd();
            CombatManager.Remove(this);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            EnemyView = new EnemyView(this, parent);
            HealthController.AddOnTakeDamage(f => EnemyView.SetHealth(HealthController));
            HealthController.AddOnHeal(f => EnemyView.SetHealth(HealthController));
            EnemyView.SetHealth(HealthController);
            ArmourLevel.AddOnValueChange(a => EnemyView.SetArmour((int) ArmourLevel.CurrentValue(), InCover));
            EnemyView.PrimaryButton.AddOnSelectEvent(() => CombatManager.SetTarget(this));
            SetDistanceData(EnemyView);
            SetConditions();
            return EnemyView;
        }


        public virtual void Alert()
        {
            if (Alerted || !InCombat()) return;
            Alerted = true;
            CombatManager.GetEnemies().ForEach(e => { e.Alert(); });
        }

        protected override void ReachTarget()
        {
            if (Alerted)
            {
                CurrentAction = Aim();
            }
            else
            {
                Wander();
            }
        }

        private void CheckForReload()
        {
            if (Weapon().Empty())
            {
                SetActionText("Reloading");
                TakeCover();
                TryReload();
                return;
            }
            if (Weapon().Cocked) return;
            SetActionText("Cocking");
            TryReload();
        }

        private Action Fire()
        {
            int noShots = (int)(Weapon().GetAttributeValue(AttributeType.Capacity) / 5f);
            bool automatic = Weapon().WeaponAttributes.Automatic;
            SetActionText("Firing");
            if (!automatic)
            {
                noShots = 1;
            }
            return () =>
            {
                Shot s = FireWeapon(CombatManager.Player());
                if (s == null) return;
                EnemyView.UiAimController.Fire();
                --noShots;
                int remainingAmmo = Weapon().GetRemainingAmmo();
                if (noShots == 0 || remainingAmmo == 0)
                {
                    EnemyView.UiAimController.SetValue(0);
                }
                if (remainingAmmo == 0 || !automatic)
                {
                    CurrentAction = CheckForReload;
                }
                else if(noShots == 0)
                {
                    CurrentAction = Aim();
                }
            };
        }
        
        protected Action Aim()
        {
            LeaveCover();
            CheckForReload();
            float aimTime = MaxAimTime;
            SetActionText("Aiming");
            return () =>
            {
                if (Immobilised()) return;
                float normalisedTime = Helper.Normalise(aimTime, MaxAimTime);
                EnemyView.UiAimController.SetValue(1 - normalisedTime);
                aimTime -= Time.deltaTime;
                if (aimTime < 0)
                {
                    CurrentAction = Fire();
                }
            };
        }

        public override void Update()
        {
            if (!InCombat()) return;
            base.Update();
            CheckAlertLevel();
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
            Burning.OnConditionNonEmpty = EnemyView.StartBurning;
            Burning.OnConditionEmpty = EnemyView.StopBurning;
            Bleeding.OnConditionNonEmpty = EnemyView.StartBleeding;
            Bleeding.OnConditionEmpty = EnemyView.StopBleeding;
            Sickening.OnConditionNonEmpty = () => EnemyView.UpdateSickness(((Sickness) Sickening).GetNormalisedValue());
            Sickening.OnConditionEmpty = () => EnemyView.UpdateSickness(0);
        }
    }
}