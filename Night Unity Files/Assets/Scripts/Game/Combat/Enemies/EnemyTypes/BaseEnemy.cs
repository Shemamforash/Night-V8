using System;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public partial class Enemy
    {
        protected float TargetDistance;
        protected Action CurrentAction;
        private readonly Cooldown _aimCooldown;
        private readonly Cooldown _fireCooldown;
        private readonly Cooldown _firingCooldown;
        private float _aimTime = 2f;
        protected float PreferredCoverDistance;
        protected float MinimumFindCoverDistance;
        private int _wanderDirection = -1;
        private Cooldown _wanderCooldown;
        protected bool AlertOthers;
        public bool AcceptsHealing = true;
        protected bool ShowMovementText = true;

        private void Wander()
        {
            if (_wanderCooldown.Running()) return;
            _wanderCooldown.Start();
        }

        protected void SetAimTime(float aimTime)
        {
            _aimTime = aimTime;
            _aimCooldown.Duration = _aimTime;
        }
        
        protected virtual void Alert()
        {
            Assert.IsFalse(_alerted);
            _alerted = true;
            _wanderCooldown.Cancel();
            if (AlertOthers)
            {
                CombatManager.GetEnemies().ForEach(e => { e.TryAlert(); });
            }
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
        }

        protected bool Moving()
        {
            return TargetDistance > 0;
        }

        protected void FindCover()
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
                if(ShowMovementText) SetActionText("Approaching");
                float newDistance = Distance.CurrentValue();
                if (!(newDistance <= TargetDistance)) return;
                ReachTarget();
            }
            else
            {
                MoveBackward();
                if(ShowMovementText) SetActionText("Retreating");
                float newDistance = Distance.CurrentValue();
                if (!(newDistance >= TargetDistance)) return;
                ReachTarget();
            }
        }

        protected virtual void ReachTarget()
        {
            Distance.SetCurrentValue(TargetDistance);
            TakeCover();
            TargetDistance = -1;
            CurrentAction = AnticipatePlayer;
        }

        protected void AnticipatePlayer()
        {
            if (Distance < MinimumFindCoverDistance)
            {
                FindCover();
                return;
            }
            CurrentAction = AimAndFire;
        }

        private void TryFire()
        {
            if (_aimCooldown.Running()) return;
            if (NeedsCocking() || Weapon().Empty())
            {
                TryReload();
                return;
            }
            FireWeapon(CombatManager.Player());
        }
        
        private void AimAndFire()
        {
            Assert.IsFalse(Moving());
            if (Immobilised()) return;
            if (Weapon().Empty())
            {
                TryReload();
                return;
            }
            if (_aimCooldown.Running() || _fireCooldown.Running()) return;
            _aimCooldown.Start();
        }

        public virtual void UpdateBehaviour()
        {
            if (!InCombat()) return;
            RageController.Decrease();
            UpdateDetection();
            if (IsDead()) return;
            CurrentAction?.Invoke();
        }
    }
}