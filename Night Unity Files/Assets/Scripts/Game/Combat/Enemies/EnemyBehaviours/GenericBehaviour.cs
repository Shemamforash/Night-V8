using System;
using System.Dynamic;
using Game.Combat.CombatStates;
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
        private Cooldown _meleeCooldown, _wanderCooldown;

        public GenericBehaviour(EnemyPlayerRelation relation) : base(nameof(GenericBehaviour), relation)
        {
            _damageBeforeCover = relation.Enemy._enemyHp.Max / relation.Enemy.BaseAttributes.GetCalculatedValue(AttributeType.Stability) * 10;
            if (relation.Enemy.BaseAttributes.GetCalculatedValue(AttributeType.Intelligence) == 0)
            {
                _isAnimal = true;
            }
            if (relation.Enemy.BaseAttributes.GetCalculatedValue(AttributeType.Stability) == 0)
            {
                _isCoward = true;
            }
            SetMeleeCooldown();
            SetWanderCooldown();
        }

        private void SetMeleeCooldown()
        {
            _meleeCooldown = CombatManager.CombatCooldowns.CreateCooldown(0.5f);
            _meleeCooldown.SetStartAction(() => SetStatusText("Meleeing"));
            _meleeCooldown.SetEndAction(() =>
            {
                if (Relation.Player.CombatController.InCover()) return;
                if (Relation.Distance > 0) return;
                Relation.Player.CombatController.KnockDown();
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
                    EnemyCombatController.Retreat();
                }
                else
                {
                    EnemyCombatController.Approach();
                }
            });
        }

        private void CheckShouldTakeCover()
        {
            if (_damageTakenSinceLastCover >= _damageBeforeCover)
            {
                if (EnemyCombatController.InCover()) return;
                EnemyCombatController.TakeCover();
                _damageTakenSinceLastCover = 0;
            }
        }

        private void CheckShouldLeaveCover()
        {
            if (!EnemyCombatController.InCover()) return;
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastPlayerLastFire;
            if (timeElapsed > 3000)
            {
                EnemyCombatController.LeaveCover();
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
                EnemyCombatController.Retreat();
                return;
            }
            if (Relation.Distance.ReachedMin())
            {
                Melee();
            }
            else if (Relation.Distance < 20)
            {
                SetStatusText("Charging");
                EnemyCombatController.StartSprinting();
                EnemyCombatController.Approach();
            }
            else
            {
                SetStatusText("Retreating");
                EnemyCombatController.StopSprinting();
                EnemyCombatController.Retreat();
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
                Wander();
            }
        }

        private void Wander()
        {
            if (_wanderCooldown.Running()) return;
            _wanderDirection = Random.Range(0, 2) == 0 ? Direction.Left : Direction.Right;
            _wanderCooldown.SetDuration(Random.Range(3, 5));
            _wanderCooldown.Start();
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