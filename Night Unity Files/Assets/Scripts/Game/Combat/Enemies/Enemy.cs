using System;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : CombatItem
    {
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        private const int EnemyReloadMultiplier = 4;

        public readonly int MaxHealth;

        protected Action CurrentAction;
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
            EnemyView.SetActionText("Waiting for Medic");
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
            EnemyView.SetActionText("Knocked Down");
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

        protected Enemy(string name, int enemyHp, int speed, float position) : base(name, speed, position)
        {
            MaxHealth = enemyHp;
            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CharacterInventory.SetEnemyResources();

            CurrentAction = Wander;
            HealthController.AddOnTakeDamage(f => { Alert(); });
            FacingDirection = Direction.Left;
        }

        public override void TakeCover()
        {
            base.TakeCover();
            EnemyView.SetArmour((int) ArmourLevel.CurrentValue(), true);
        }

        public override void LeaveCover()
        {
            base.LeaveCover();
            EnemyView.SetArmour((int) ArmourLevel.CurrentValue(), true);
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
        private const int MeleeDistance = 5;

        protected Action CheckForRepositioning(bool moveAnyway = false)
        {
            if (!Alerted || !InCombat()) return null;
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
            EnemyView.SetActionText("Meleeing");
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
            EnemyView.SetActionText("Staggered");
            return () =>
            {
                currentTime -= Time.deltaTime;
                if (currentTime > 0) return;
                ChooseNextAction();
            };
        }

        private bool _aheadOfPlayer;

        protected Action MoveToTargetDistance(float distance)
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
                EnemyView.SetActionText(DistanceToPlayer > distance ? "Approaching" : "Retreating");
                return DistanceToPlayer > distance ? moveForwardAction : moveBackwardAction;
            }

            EnemyView.SetActionText(DistanceToPlayer > distance ? "Retreating" : "Approaching");
            return DistanceToPlayer > distance ? moveBackwardAction : moveForwardAction;
        }

        private void Wander()
        {
            float distanceToTravel = Random.Range(0f, 5f);
            distanceToTravel *= _wanderDirection;
            _wanderDirection = -_wanderDirection;
            float targetPosition = Position.CurrentValue() + distanceToTravel;
            CurrentAction = MoveToTargetPosition(targetPosition);
            EnemyView.SetActionText("Wandering");
            CheckForPlayer();
        }

        private float CalculateIdealRange()
        {
            if (Weapon() == null) return 0;
            float idealRange = Weapon().GetAttributeValue(AttributeType.Accuracy);
            idealRange = Random.Range(0.8f * idealRange, idealRange);
            if (idealRange >= CombatManager.VisibilityRange)
            {
                idealRange = Random.Range(0.8f * CombatManager.VisibilityRange, CombatManager.VisibilityRange);
            }

            return idealRange;
        }

        private void CheckForPlayer()
        {
            if (DistanceToPlayer < _visionRange)
            {
                CurrentAction = Suspicious;
            }
        }

        private void Suspicious()
        {
            EnemyView.SetActionText("Suspicious");
            if (DistanceToPlayer >= _detectionRange.CurrentValue()) return;
            Alert();
        }

        public bool InCombat()
        {
            return !HasFled && !IsDead;
        }

        public override void OnHit(int damage, bool isCritical)
        {
            base.OnHit(damage, isCritical);
            if (!Alerted) Alert();
        }

        public override void OnMiss()
        {
            if (!Alerted) Alert();
        }

        public override void Kill()
        {
            IsDead = true;
            UIEnemyController.Remove(this);
            EnemyView?.MarkDead();
            CombatManager.CheckCombatEnd();
            CurrentAction = null;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            EnemyView = new EnemyView(this, parent);
            HealthController.AddOnTakeDamage(f => EnemyView.UpdateHealth());
            HealthController.AddOnHeal(f => EnemyView.UpdateHealth());
            EnemyView.UpdateHealth();
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
            if (Weapon().GetRemainingMagazines() == 0)
            {
                return Flee();
            }

            TakeCover();
            EnemyView.SetActionText("Reloading");
            float duration = EquipmentController.Weapon().GetAttributeValue(AttributeType.ReloadSpeed) * EnemyReloadMultiplier;
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                EquipmentController.Weapon().Reload(Inventory());
                ChooseNextAction();
            };
        }

        private Action Flee()
        {
            EnemyView.SetActionText("Fleeing");
            return () =>
            {
                MovementController.MoveBackward();
                CombatManager.CheckEnemyFled(this);
            };
        }

        private Action Fire()
        {
            int divider = Random.Range(3, 6);
            int noShots = (int) (Weapon().GetAttributeValue(AttributeType.Capacity) / divider);
            bool automatic = Weapon().WeaponAttributes.Automatic;
            EnemyView.SetActionText("Firing");
            if (!automatic)
            {
                noShots = 1;
            }

            return () =>
            {
                Shot s = FireWeapon(CombatManager.Player);
                if (s == null) return;
                s.Fire();
                EnemyView.UiAimController.Fire();
                --noShots;
                int remainingAmmo = Weapon().GetRemainingAmmo();
                if (noShots == 0 || remainingAmmo == 0)
                {
                    EnemyView.UiAimController.SetValue(0);
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

        protected virtual Action Aim()
        {
            LeaveCover();
            Assert.IsFalse(Weapon().Empty());
            float aimTime = MaxAimTime;
            EnemyView.SetActionText("Aiming");
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
            CurrentAction?.Invoke();
        }

        protected override void SetConditions()
        {
            base.SetConditions();
            Burning.OnConditionNonEmpty = EnemyView.HealthBar.StartBurning;
            Burning.OnConditionEmpty = EnemyView.HealthBar.StopBurning;
            Bleeding.OnConditionNonEmpty = EnemyView.HealthBar.StartBleeding;
            Bleeding.OnConditionEmpty = EnemyView.HealthBar.StopBleeding;
            Sickening.OnConditionNonEmpty = () => EnemyView.HealthBar.UpdateSickness(((Sickness) Sickening).GetNormalisedValue());
            Sickening.OnConditionEmpty = () => EnemyView.HealthBar.UpdateSickness(0);
        }
    }
}