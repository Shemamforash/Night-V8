using System;
using NUnit.Framework;
using SamsHelper.ReactiveUI;

namespace Game.Combat.Misc
{
	public class HealthController
	{
		private readonly Number        _healthRemaining = new Number();
		private          CanTakeDamage _healthyThing;
		private          Action<float> OnHeal;
		private          Action        OnKill;
		private event Action<float>    OnTakeDamage;

		public void SetInitialHealth(int initialHealth, CanTakeDamage character, int maxHealth = -1)
		{
			_healthyThing                 = character;
			_healthRemaining.Max          = maxHealth == -1 ? initialHealth : maxHealth;
			_healthRemaining.CurrentValue = initialHealth;
		}

		public void TakeDamage(float amount)
		{
			Assert.IsTrue(amount >= 0);
			if (amount == 0) return;
			if (_healthRemaining.ReachedMin) return;
			_healthRemaining.Increment(-amount);
			OnTakeDamage?.Invoke(amount);
			if (!_healthRemaining.ReachedMin) return;
			OnKill?.Invoke();
			_healthyThing.Kill();
		}

		public void Heal(int amount)
		{
			Assert.IsTrue(amount >= 0);
			_healthRemaining.Increment(amount);
			OnHeal?.Invoke(amount);
		}

		public void AddOnTakeDamage(Action<float> a) => OnTakeDamage += a;

		public void SetOnKill(Action a) => OnKill = a;

		public void   SetOnHeal(Action<float> a) => OnHeal = a;
		public float  GetNormalisedHealthValue() => _healthRemaining.Normalised;
		public float  GetCurrentHealth()         => _healthRemaining.CurrentValue;
		public float  GetMaxHealth()             => _healthRemaining.Max;
		public Number GetHealth()                => _healthRemaining;
	}
}