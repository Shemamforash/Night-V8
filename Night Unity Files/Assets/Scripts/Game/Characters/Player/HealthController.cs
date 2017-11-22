using System;
using SamsHelper.ReactiveUI;

namespace Game.Characters
{
    public class HealthController
    {
        private readonly MyValue _healthRemaining;
        private event Action<int> OnTakeDamage;
        private event Action<int> OnHeal;

        public HealthController(Character character)
        {
            _healthRemaining = character.BaseAttributes.Strength;
            _healthRemaining?.OnMin(character.Kill);
        }

        public void TakeDamage(int amount)
        {
//            CombatManager.CombatUi.UpdateCharacterHealth(BaseAttributes.Strength);
            _healthRemaining.Decrement(amount);
            OnTakeDamage?.Invoke(amount);
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
    }
}