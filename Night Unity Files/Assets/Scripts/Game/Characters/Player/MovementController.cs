using System;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Characters.Player
{
    public class MovementController
    {
        private Character _character;
        protected Cooldown DashCooldown;
        private const float DashDuration = 2f;
        private int baseSpeed;
        protected readonly int SprintModifier = 2;
        private bool _sprinting;
        private Action<float> _moveForwardAction, _moveBackwardAction;

        public MovementController(Character character, int speed)
        {
            _character = character;
            SetDashCooldown();
            baseSpeed = speed;
            Player p = _character as Player;
            if (p != null)
            {
                _moveForwardAction = f => CombatManager.GetEnemies().ForEach(e => e.Distance.Decrement(f));
                _moveBackwardAction = f => CombatManager.GetEnemies().ForEach(e => e.Distance.Increment(f));
                DashCooldown.SetController(CombatManager.DashCooldownController);
            }
            else
            {
                _moveForwardAction = f => ((Enemy)_character).Distance.Decrement(f);
                _moveBackwardAction = f => ((Enemy)_character).Distance.Increment(f);
            }
        }
        
        private void SetDashCooldown()
        {
            DashCooldown = CombatManager.CombatCooldowns.CreateCooldown(DashDuration);
        }
        
        //MOVEMENT

        private float CurrentSpeed()
        {
            float currentSpeed = baseSpeed;
            if (_sprinting) currentSpeed *= SprintModifier;
            return currentSpeed * Time.deltaTime;
        }

        public void MoveForward()
        {
            Move(1);
        }

        public void MoveBackward()
        {
            Move(-1);
        }
        
        public void Move(float direction)
        {
            if (_character.Immobilised()) return;
            _character.LeaveCover();
            float distanceToMove = CurrentSpeed();
            if (direction > 0)
            {
                _moveForwardAction?.Invoke(distanceToMove);
            }
            else
            {
                _moveBackwardAction?.Invoke(distanceToMove);
            }
        }

        public void KnockBack(float distance)
        {
            _moveBackwardAction?.Invoke(distance);
        }
        
        //DASHING
        
        public void DashForward()
        {
            Dash(1);
        }

        public void DashBackward()
        {
            Dash(-1);
        }
        
        public void Dash(float direction)
        {
            if (_character.Immobilised() || !CanDash()) return;
            _character.LeaveCover();
            if (direction > 0)
            {
                _moveForwardAction?.Invoke(_dashDistance);
            }
            else
            {
                _moveBackwardAction?.Invoke(_dashDistance);
            }
            DashCooldown.Start();
        }
        
        private bool CanDash()
        {
            return DashCooldown.Finished();
        }

        private float _dashDistance = 5f;

        //SPRINTING

        public void StartSprinting()
        {
            if (_sprinting) return;
            _sprinting = true;
        }

        public void StopSprinting()
        {
            if (!_sprinting) return;
            _sprinting = false;
        }
    }
}