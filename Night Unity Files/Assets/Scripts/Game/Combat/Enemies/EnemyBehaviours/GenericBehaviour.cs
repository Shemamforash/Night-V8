using System;
using System.Collections.Generic;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
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

        public GenericBehaviour(EnemyPlayerRelation relation) : base(nameof(GenericBehaviour), relation)
        {
            _damageBeforeCover = relation.Enemy._enemyHp.Max / relation.Enemy.BaseAttributes.GetCalculatedValue(AttributeType.Stability) * 10;
            _isAnimal = relation.Enemy.BaseAttributes.Intelligence.ReachedMin();
            _isCoward = relation.Enemy.BaseAttributes.Stability.ReachedMin();
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
                Relation.Enemy.AddVisionModifier(-0.5f);
                SetStatusText("Grazing");
            });
            _grazeCooldown.SetEndAction(() => Relation.Enemy.RemoveVisionModifier(-0.5f), true);
        }
        
        private void SetWatchCooldown()
        {
            _watchCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _watchCooldown.SetStartAction(() =>
            {
                Relation.Enemy.AddVisionModifier(1f);
                SetStatusText("Watching");
            });
            _watchCooldown.SetEndAction(() => Relation.Enemy.RemoveVisionModifier(1f), true);
        }

        private void SetMeleeCooldown()
        {
            _meleeCooldown = CombatManager.CombatCooldowns.CreateCooldown(0.5f);
            _meleeCooldown.SetStartAction(() => SetStatusText("Meleeing"));
            _meleeCooldown.SetEndAction(() =>
            {
                if (Relation.Player.InCover()) return;
                if (Relation.Distance > 0) return;
                Relation.Player.KnockDown();
                Relation.Player.TakeDamage(10);
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
                    Relation.Enemy.Retreat();
                }
                else
                {
                    Relation.Enemy.Approach();
                }
            });
        }

        private void CheckShouldTakeCover()
        {
            if (_damageTakenSinceLastCover >= _damageBeforeCover)
            {
                if (Relation.Enemy.InCover()) return;
                Relation.Enemy.TakeCover();
                _damageTakenSinceLastCover = 0;
            }
        }

        private void CheckShouldLeaveCover()
        {
            if (!Relation.Enemy.InCover()) return;
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastPlayerLastFire;
            if (timeElapsed > 3000)
            {
                Relation.Enemy.LeaveCover();
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
                Relation.Enemy.StartSprinting();
                Relation.Enemy.Retreat();
                return;
            }
            if (Relation.Distance.ReachedMin())
            {
                Melee();
            }
            else if (Relation.Distance < 20)
            {
                SetStatusText("Charging");
                Relation.Enemy.StartSprinting();
                Relation.Enemy.Approach();
            }
            else
            {
                SetStatusText("Retreating");
                Relation.Enemy.StopSprinting();
                Relation.Enemy.Retreat();
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
            if (Relation.Enemy.IsAlerted())
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