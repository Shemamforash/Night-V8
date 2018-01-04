using System;
using SamsHelper.ReactiveUI;

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

        public HealthController(Character character)
        {
            _character = character;
        }

        public void EnterCombat()
        {
            _healthRemaining = (int) (_character.BaseAttributes.Strength.CurrentValue() * _healthChunkSize);
            _healthAtStartOfCombat = _healthRemaining;
            HeartBeatController.Enable();

        }

        public void TakeDamage(int amount)
        {
//            CombatManager.CombatUi.UpdateCharacterHealth(BaseAttributes.Strength);
            _healthRemaining -= amount;
            if (_healthRemaining <= 0)
            {
                _healthRemaining = 0;
                //die in combat
            }
            OnTakeDamage?.Invoke(amount);
            float percentHealthRemaining = (float)_healthRemaining / _healthAtStartOfCombat;
            HeartBeatController.SetHealth(percentHealthRemaining);
        }

        public void ExitCombat()
        {
            _character.BaseAttributes.Strength.SetCurrentValue(_healthRemaining / _healthChunkSize);
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
            OnHeal?.Invoke(amount);
        }

        public void AddOnTakeDamage(Action<int> a) => OnTakeDamage += a;
        public void AddOnHeal(Action<int> a) => OnHeal += a;
    }
}