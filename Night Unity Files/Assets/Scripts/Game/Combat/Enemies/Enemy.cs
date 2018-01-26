using System;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Combat.Skills;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : CombatItem
    {
        protected readonly CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        protected readonly CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);

        public readonly int MaxHealth;

        protected Action CurrentAction;
        private readonly Cooldown _firingCooldown;
        private const float MaxAimTime = 2f;
        protected float MinimumFindCoverDistance;
        private int _wanderDirection = -1;

        public bool HasFled;
        protected bool Alerted;

        public EnemyView EnemyView;

        private const float KnockdownDuration = 3f;
        private bool _waitingForHeal;

        private void SetHealBehaviour()
        {
            HealthController.AddOnTakeDamage(a =>
            {
                if (HealthController.GetNormalisedHealthValue() > 0.25f || _waitingForHeal) return;
                foreach (Enemy enemy in CombatManager.GetEnemies())
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
                CurrentAction = Aim();
            };
        }

        public void ClearHealWait()
        {
            CurrentAction = Aim();
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            CurrentAction = Aim();
        }

        public override void KnockDown()
        {
            if (!IsKnockedDown) return;
            base.KnockDown();
            CurrentAction = KnockedDown();
        }
        
        private Action KnockedDown()
        {
            float duration = KnockdownDuration;
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                CurrentAction = ChooseNextAction();
                IsKnockedDown = false;
            };
        }

        protected virtual Action ChooseNextAction()
        {
            return Aim();
        }

        protected Enemy(string name, int enemyHp, int speed, float position) : base(name, speed, position)
        {
            MaxHealth = enemyHp;
            if (!(this is Medic || this is Martyr)) SetHealBehaviour();
            CharacterInventory.SetEnemyResources();

            CurrentAction = Wander;
            HealthController.AddOnTakeDamage(f => { Alert(); });
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
                if (DistanceToPlayer <= MaxDistance)
                {
                    CheckForRepositioning();
                }
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
            CheckForPlayer();
        }

        public void CheckForRepositioning(bool moveAnyway = false)
        {
            if (!Alerted || !InCombat()) return;
            if (DistanceToPlayer < MinimumFindCoverDistance || DistanceToPlayer > CombatManager.VisibilityRange || moveAnyway)
            {
                float targetDistance = CalculateIdealRange();
                CurrentAction = MoveToTargetDistance(targetDistance);
            }
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
            if (DistanceToPlayer < VisionRange)
            {
                CurrentAction = Suspicious;
            }
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToPlayer >= DetectionRange.CurrentValue()) return;
            Alert();
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

        public override void OnHit(int damage, bool isCritical)
        {
            base.OnHit(damage, isCritical);
            if(!Alerted) Alert();
        }
        
        public override void OnMiss()
        {
            if(!Alerted) Alert();
        }

        public override void Kill()
        {
            IsDead = true;
            CombatManager.EnemyList.Remove(EnemyView);
            EnemyView?.MarkDead();
            CombatManager.CheckCombatEnd();
            CombatManager.Remove(this);
            CurrentAction = null;
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
            CheckForRepositioning(true);
        }

        protected override void ReachTarget()
        {
            if (Alerted)
            {
                CurrentAction = ChooseNextAction();
            }
            else
            {
                Wander();
            }
        }

        private Action Cock()
        {
            SetActionText("Cocking");
            float duration = EquipmentController.Weapon().GetAttributeValue(AttributeType.FireRate);
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                EquipmentController.Weapon().Cocked = true;
                CurrentAction = Aim();
            };
        }
        
        private Action Reload()
        {
            if (Weapon().GetRemainingMagazines() == 0)
            {
                return Flee();
            }
            TakeCover();
            SetActionText("Reloading");
            float duration = EquipmentController.Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            return () =>
            {
                duration -= Time.deltaTime;
                if (duration > 0) return;
                EquipmentController.Weapon().Cocked = true;
                EquipmentController.Weapon().Reload(Inventory());
                CurrentAction = Aim();
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
            int noShots = (int)(Weapon().GetAttributeValue(AttributeType.Capacity) / divider);
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
                    CurrentAction = CheckForReload();
                }
                else if(noShots == 0)
                {
                    CurrentAction = Aim();
                }
            };
        }
        
        private Action CheckForReload()
        {
            if (Weapon().Empty())
            {
                return Reload();
            }
            return Weapon().Cocked ? null : Cock();
        }
        
        protected virtual Action Aim()
        {
            LeaveCover();
            Action reloadAction = CheckForReload();
            if (reloadAction != null) return reloadAction;
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