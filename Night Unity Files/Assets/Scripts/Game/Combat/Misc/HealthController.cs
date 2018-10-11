using System;
using NUnit.Framework;
using SamsHelper.ReactiveUI;

namespace Game.Combat.Misc
{
    public class HealthController
    {
        private readonly Number _healthRemaining = new Number();
        private CanTakeDamage _healthyThing;
        private event Action<float> OnTakeDamage;
        private Action<float> OnHeal;
        private Action OnKill;

        public void SetInitialHealth(int initialHealth, CanTakeDamage character, int maxHealth = -1)
        {
            _healthyThing = character;
            _healthRemaining.Max = maxHealth == -1 ? initialHealth : maxHealth;
            _healthRemaining.SetCurrentValue(initialHealth);
        }

        public void TakeDamage(float amount)
        {
            Assert.IsTrue(amount >= 0);
            if (amount == 0) return;
            if (_healthRemaining.ReachedMin()) return;
            _healthRemaining.Decrement(amount);
            OnTakeDamage?.Invoke(amount);
            if (!_healthRemaining.ReachedMin()) return;
            OnKill?.Invoke();
            _healthyThing.Kill();
        }

        public void Heal(int amount)
        {
            Assert.IsTrue(amount >= 0);
            _healthRemaining.Increment(amount);
            OnHeal?.Invoke(amount);
        }

        public void AddOnTakeDamage(Action<float> a)
        {
            OnTakeDamage += a;
        }

        public void SetOnKill(Action a)
        {
            OnKill = a;
        }

        public void SetOnHeal(Action<float> a)
        {
            OnHeal = a;
        }

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

        public Number GetHealth()
        {
            return _healthRemaining;
        }
    }
}