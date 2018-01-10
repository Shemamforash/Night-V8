using System;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class HealthController
    {
        private int _healthRemaining;
        private event Action<int> OnTakeDamage;
        private event Action<int> OnHeal;
        private Character _character;
        private readonly int _healthChunkSize = 10;
        private int _healthAtStartOfCombat;
        private float _normalisedHealth;

        public HealthController(Character character)
        {
            _character = character;
        }

        public void EnterCombat()
        {
            int strengthRating;
            if (_character is Player.Player)
            {
                strengthRating = (int) ((Player.Player) _character).Attributes.Strength.CurrentValue();
            }
            else
            {
                strengthRating = ((Enemy) _character).MaxHealth;
            }
            _healthRemaining = strengthRating * _healthChunkSize;
            _healthAtStartOfCombat = _healthRemaining;
            HeartBeatController.Enable();
            RecalculateNormalisedHealth();
        }

        public void TakeDamage(int amount)
        {
            if (_healthRemaining <= 0) return;
            _healthRemaining -= amount;
            if (_healthRemaining <= 0)
            {
                _healthRemaining = 0;
                _character.Kill();
            }
            RecalculateNormalisedHealth();
            OnTakeDamage?.Invoke(amount);
            HeartBeatController.SetHealth(_normalisedHealth);
            if(_character is Player.Player) ;
        }

        private void RecalculateNormalisedHealth()
        {
            _normalisedHealth = (float)_healthRemaining / _healthAtStartOfCombat;
        }

        public void ExitCombat()
        {
            if (_character is Player.Player)
            {
                ((Player.Player) _character).Attributes.Strength.SetCurrentValue(_healthRemaining / _healthChunkSize);
            }
            HeartBeatController.Disable();
        }
        
        public void TakeCriticalDamage(int amount)
        {
            TakeDamage(amount);
            OnTakeDamage?.Invoke(amount);
        }

        public void Heal(int amount)
        {
            int newAmount = _healthRemaining + amount;
            if (newAmount > _healthAtStartOfCombat)
            {
                amount = newAmount - _healthAtStartOfCombat;
            }
            _healthRemaining += amount;
            RecalculateNormalisedHealth();
            OnHeal?.Invoke(amount);
        }

        public void AddOnTakeDamage(Action<int> a) => OnTakeDamage += a;
        public void AddOnHeal(Action<int> a) => OnHeal += a;

        public float GetNormalisedHealthValue()
        {
            return _normalisedHealth;
        }

        public float GetCurrentHealth()
        {
            return _healthRemaining;
        }

        public float GetMaxHealth()
        {
            return _healthAtStartOfCombat;
        }
    }
}