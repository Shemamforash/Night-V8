using System;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;

namespace Game.Characters.Player
{
    public class MovementController
    {
        private readonly Character _character;
        private Cooldown _dashCooldown;
        private const float DashDuration = 2f;
        private float _baseSpeed;
        private const int SprintModifier = 2;
        private bool _sprinting;
        private readonly Action<float> _moveForwardAction, _moveBackwardAction;

        public void SetSpeed(int speed)
        {
            _baseSpeed = speed;
        }

        public float GetDashCooldown()
        {
            return Helper.Round(_dashCooldown.Duration, 1);
        }

        public float GetSpeed()
        {
            return _baseSpeed;
        }
        
        public MovementController(Character character, int speed)
        {
            _character = character;
            SetDashCooldown();
            _baseSpeed = speed;
            Player player = character as Player;
            if (player != null)
            {
                player.Attributes.Endurance.AddOnValueChange(a =>
                {
                    _dashCooldown.Duration = 2f - 0.1f * a.CurrentValue();
                    _baseSpeed = 1 + a.CurrentValue() * 0.4f;
                });
                _moveForwardAction = f => _character.Position.Increment(f);
                _moveBackwardAction = f => _character.Position.Decrement(f);
                _dashCooldown.SetDuringAction(a =>
                {
                    float normalisedTime = a / _dashCooldown.Duration;
                    RageBarController.UpdateDashTimer(1 - normalisedTime);
                });
                _dashCooldown.SetEndAction(() =>
                {
                    RageBarController.UpdateDashTimer(1);
                    RageBarController.PlayFlash();
                });
            }
            else if (_character is Enemy)
            {
                _moveForwardAction = f =>
                {
                    _character.Position.Decrement(f);
                    _character.FacingDirection = Direction.Left;
                };
                _moveBackwardAction = f =>
                {
                    _character.Position.Increment(f);
                    _character.FacingDirection = Direction.Right;
                };
            }
            else
            {
                _moveForwardAction = f => _character.Position.Decrement(f);
                _moveBackwardAction = f => _character.Position.Increment(f);
            }
        }

        private void SetDashCooldown()
        {
            _dashCooldown = CombatManager.CombatCooldowns.CreateCooldown(DashDuration);
        }

        //MOVEMENT

        private float CurrentSpeed()
        {
            float currentSpeed = _baseSpeed;
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

            _dashCooldown.Start();
        }

        private bool CanDash()
        {
            return _dashCooldown.Finished();
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