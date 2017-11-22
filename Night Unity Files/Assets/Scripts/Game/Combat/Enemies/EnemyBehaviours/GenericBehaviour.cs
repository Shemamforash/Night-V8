using System;
using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public class GenericBehaviour : EnemyBehaviour
    {
        private readonly float _damageBeforeCover;
        private float _damageTakenSinceLastCover;
        private long _timeAtLastPlayerLastFire;
        private Direction _wanderDirection;
        private readonly bool _isAnimal, _isCoward;
        private Cooldown _meleeCooldown, _wanderCooldown, _grazeCooldown, _watchCooldown;
        private Cooldown _currentIdleAction;
        private readonly List<Action> _idleActions = new List<Action>();

        public GenericBehaviour(Enemy enemy) : base(nameof(GenericBehaviour), enemy)
        {
            _damageBeforeCover = enemy.BaseAttributes.Strength.Max / enemy.BaseAttributes.GetCalculatedValue(AttributeType.Stability) * 10;
            _isAnimal = enemy.BaseAttributes.Intelligence.ReachedMin();
            _isCoward = enemy.BaseAttributes.Stability.ReachedMin();
            _idleActions.Add(Wander);
            SetMeleeCooldown();
            SetWanderCooldown();
            SetGrazeCooldown();
            SetWatchCooldown();
            _currentIdleAction = _wanderCooldown;
        }

        public void EnableGrazing()
        {
            _idleActions.Add(StartGrazing);
        }

        public void EnableWatching()
        {
            _idleActions.Add(StartWatching);
        }

        private void StartGrazing()
        {
            if (_grazeCooldown.Running()) return;
            _grazeCooldown.Duration = Random.Range(5, 10);
            _grazeCooldown.Start();
            _currentIdleAction = _grazeCooldown;
        }
        
        private void StartWatching()
        {
            if (_watchCooldown.Running()) return;
            _watchCooldown.Duration = Random.Range(5, 10);
            _watchCooldown.Start();
            _currentIdleAction = _watchCooldown;
        }

        private void SetGrazeCooldown()
        {
            _grazeCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _grazeCooldown.SetStartAction(() =>
            {
                Enemy.AddVisionModifier(-0.5f);
                SetStatusText("Grazing");
            });
            _grazeCooldown.SetEndAction(() => Enemy.RemoveVisionModifier(-0.5f), true);
        }
        
        private void SetWatchCooldown()
        {
            _watchCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _watchCooldown.SetStartAction(() =>
            {
                Enemy.AddVisionModifier(1f);
                SetStatusText("Watching");
            });
            _watchCooldown.SetEndAction(() => Enemy.RemoveVisionModifier(1f), true);
        }

        private void SetMeleeCooldown()
        {
            _meleeCooldown = CombatManager.CombatCooldowns.CreateCooldown(0.5f);
            _meleeCooldown.SetStartAction(() => SetStatusText("Meleeing"));
            _meleeCooldown.SetEndAction(() =>
            {
                if (CombatManager.Player().InCover()) return;
                //TODO melee range
                if (Enemy.Distance > 0) return;
                CombatManager.Player().KnockDown();
                CombatManager.Player().HealthController.TakeDamage(10);
            });
        }

        private void SetWanderCooldown()
        {
            _wanderCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _wanderCooldown.SetStartAction(() => SetStatusText("Wandering"));
            _wanderCooldown.SetDuringAction(f =>
            {
                if (_wanderDirection == Direction.Left)
                {
                    Enemy.MoveBackward();
                }
                else
                {
                    Enemy.MoveForward();
                }
            });
        }

        private void CheckShouldTakeCover()
        {
            if (_damageTakenSinceLastCover >= _damageBeforeCover)
            {
                if (Enemy.InCover()) return;
                Enemy.TakeCover();
                _damageTakenSinceLastCover = 0;
            }
        }

        private void CheckShouldLeaveCover()
        {
            if (!Enemy.InCover()) return;
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastPlayerLastFire;
            if (timeElapsed > 3000)
            {
                Enemy.LeaveCover();
            }
        }

        private void Melee()
        {
            if (_meleeCooldown.Running()) return;
            _meleeCooldown.Start();
        }

        private bool _charging, _retreating, _fleeing;

        private void RushPlayer()
        {
            if (_isCoward)
            {
                SetStatusText("Fleeing");
                Enemy.StartSprinting();
                Enemy.MoveBackward();
                return;
            }
            if (Enemy.Distance.ReachedMin())
            {
                Melee();
            }
            else if (Enemy.Distance.CurrentValue() < 20)
            {
                SetStatusText("Charging");
                Enemy.StartSprinting();
                Enemy.MoveForward();
            }
            else
            {
                SetStatusText("Retreating");
                Enemy.StopSprinting();
                Enemy.MoveBackward();
            }
        }

        public override void OnDetect()
        {
        }

        private void Charge()
        {
        }

        private bool Immobilised()
        {
            return _meleeCooldown.Running();
        }

        public override void Update()
        {
            if (Immobilised()) return;
            if (Enemy.IsAlerted())
            {
                if (_isAnimal)
                {
                    RushPlayer();
                }
                else
                {
                    CheckShouldTakeCover();
                    CheckShouldLeaveCover();
                }
            }
            else
            {
                _currentIdleAction.Update();
                if (_currentIdleAction.Running()) return;
                Helper.RandomInList(_idleActions)();
            }
        }

        private void Wander()
        {
            if (_wanderCooldown.Running()) return;
            _wanderDirection = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;
            _wanderCooldown.Duration = Random.Range(3, 5);
            _wanderCooldown.Start();
            _currentIdleAction = _wanderCooldown;
        }

        public void TakeDamage(int amount)
        {
            _damageTakenSinceLastCover += amount;
        }

        public void TakeFire()
        {
            _timeAtLastPlayerLastFire = Helper.TimeInMillis();
        }
    }
}