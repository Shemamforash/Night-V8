using System;
using Game.Combat.Enemies;
using SamsHelper.ReactiveUI;

namespace Game.Characters.Player
{
    public class HealthController
    {
        private Number _healthRemaining;
        private event Action<int> OnTakeDamage;
        private event Action<int> OnHeal;
        private readonly Character _character;
        private const int PlayerHealthChunkSize = 100;

        public HealthController(Character character)
        {
            _character = character;
        }

        public void EnterCombat()
        {
            int maxHealth;
            Player player = _character as Player;
            if (player != null)
            {
                maxHealth = (int) player.Attributes.Strength.CurrentValue() * PlayerHealthChunkSize;
            }
            else
            {
                maxHealth = ((Enemy) _character).MaxHealth * 10;
            }
            _healthRemaining = new Number(maxHealth, 0, maxHealth);
            _healthRemaining.OnMin(_character.Kill);
            HeartBeatController.Enable();
            HeartBeatController.SetHealth(_healthRemaining.Normalised());
        }

        public void TakeDamage(int amount)
        {
            if (_healthRemaining.ReachedMin()) return;
            _healthRemaining.Decrement(amount);
            OnTakeDamage?.Invoke(amount);
        }

        public void ExitCombat()
        {
            (_character as Player)?.Attributes.Strength.SetCurrentValue(_healthRemaining / PlayerHealthChunkSize);
            HeartBeatController.Disable();
        }

        public void TakeCriticalDamage(int amount)
        {
            TakeDamage(amount);
            OnTakeDamage?.Invoke(amount);
        }

        public void Heal(int amount)
        {
            _healthRemaining.Increment(amount);
            OnHeal?.Invoke(amount);
        }

        public void AddOnTakeDamage(Action<int> a) => OnTakeDamage += a;
        public void AddOnHeal(Action<int> a) => OnHeal += a;

        public float GetNormalisedHealthValue()
        {
            return _healthRemaining.Normalised();
        }

        public float GetCurrentHealth()
        {
            return _healthRemaining.CurrentValue();
        }

        public float GetMaxHealth()
        {
            return _healthRemaining.Max;
        }
    }
}