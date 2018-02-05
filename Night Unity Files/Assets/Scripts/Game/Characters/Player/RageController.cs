using Game.Combat;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class RageController : ICombatListener
    {
        private readonly Number _rageLevel = new Number(0, 0, 1);
        private bool _activated;
        private readonly Character _character;
        private readonly AttributeModifier _reloadModifier = new AttributeModifier();
        private readonly AttributeModifier _fireRateModifier = new AttributeModifier();
            
        public RageController(Character character)
        {
            _reloadModifier.SetMultiplicative(0.5f);
            _fireRateModifier.SetMultiplicative(2f);
            _character = character;
            _rageLevel.OnMin(End);
            if (_character is Player.Player)
            {
                _rageLevel.AddOnValueChange(a => RageBarController.SetRageBarFill(a.CurrentValue(), _activated));
            }
            CombatManager.RegisterCombatListener(this);
        }

        public float CurrentValue()
        {
            return _rageLevel.CurrentValue();
        }
        
        public void Increase(float damage)
        {
            if (!_activated)
            {
                _rageLevel.Increment(0.01f * damage);
            }
        }

        public bool Active() => _activated;

        public bool Spend(float amount)
        {
            if (!(_rageLevel.CurrentValue() >= amount)) return false;
            _rageLevel.Decrement(amount);
            return true;
        }
        
        public bool Decrease()
        {
            float decreaseAmount = 0f;
            if (_rageLevel.CurrentValue() < 1 && !_activated)
            {
                decreaseAmount = -0.04f;
            }
            else if (_activated)
            {
                decreaseAmount = -0.1f;
            }
            _rageLevel.Increment(decreaseAmount * Time.deltaTime);
            return !_rageLevel.ReachedMin();
        }

        public void End()
        {
            if (!_activated) return;
            _activated = false;
            _reloadModifier.Remove();
            _fireRateModifier.Remove();
        }

        public void TryStart()
        {
            if (_rageLevel.CurrentValue() != 1) return;
            _activated = true;
            _reloadModifier.AddTargetAttribute(_character.Weapon().WeaponAttributes.ReloadSpeed);
            _fireRateModifier.AddTargetAttribute(_character.Weapon().WeaponAttributes.FireRate);
            _reloadModifier.Apply();
            _fireRateModifier.Apply();
        }

        public void EnterCombat()
        {
            _rageLevel.SetCurrentValue(0f);
        }

        public void ExitCombat()
        {
        }

        public void UpdateCombat()
        {
            if (_rageLevel.ReachedMax()) return;
            _rageLevel.Decrement(Time.deltaTime * 0.05f);
        }
    }
}