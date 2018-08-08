using System;
using Game.Characters;
using Game.Combat.Player;
using Game.Combat.Ui;
using NUnit.Framework;
using SamsHelper.ReactiveUI;

namespace Game.Combat.Misc
{
    public class HealthController
    {
        private readonly Number _healthRemaining = new Number();
        private ITakeDamageInterface _healthyThing;
        private event Action<float> OnTakeDamage;
        private event Action<float> OnHeal;

        private UIHealthBarController GetHealthBarController()
        {
            CharacterCombat c = _healthyThing as CharacterCombat;
            if (c == null) return null;
            if (_healthyThing is PlayerCombat) return PlayerUi.Instance().GetHealthController(c);
            return EnemyUi.Instance().GetHealthController(c);
        }

        public void SetInitialHealth(int initialHealth, ITakeDamageInterface character)
        {
            _healthyThing = character;
            _healthRemaining.Max = initialHealth;
            _healthRemaining.SetCurrentValue(initialHealth);
            UpdateHealth();
        }

        public void UpdateHealth()
        {
            GetHealthBarController()?.SetValue(_healthRemaining);
        }

        public void TakeDamage(float amount)
        {
            Assert.IsTrue(amount >= 0);
            if (amount == 0) return;
            if (_healthRemaining.ReachedMin()) return;
            _healthRemaining.Decrement(amount);
            GetHealthBarController()?.FadeNewHealth();
            OnTakeDamage?.Invoke(amount);
            if (_healthRemaining.ReachedMin()) _healthyThing.Kill();
            UpdateHealth();
        }

        public void Heal(int amount)
        {
            Assert.IsTrue(amount >= 0);
            _healthRemaining.Increment(amount);
            OnHeal?.Invoke(amount);
            UpdateHealth();
        }

        public void AddOnTakeDamage(Action<float> a)
        {
            OnTakeDamage += a;
        }

        public void AddOnHeal(Action<float> a)
        {
            OnHeal += a;
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
    }
}