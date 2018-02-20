using Game.Combat;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters.Player
{
    public class RageController : ICombatListener
    {
        private readonly Number _rageLevel = new Number(0, 0, 8);
        private bool _activated;
        private readonly AttributeModifier _reloadModifier = new AttributeModifier();
        private readonly AttributeModifier _fireRateModifier = new AttributeModifier();

        public RageController()
        {
            _reloadModifier.SetMultiplicative(0.5f);
            _fireRateModifier.SetMultiplicative(2f);
            _rageLevel.OnMin(End);
            _rageLevel.AddOnValueChange(a => RageBarController.SetRageBarFill(a.Normalised(), _activated));
        }

        public float CurrentValue()
        {
            return _rageLevel.CurrentValue();
        }

        public void Increase(float damage)
        {
            if (!_activated)
            {
                _rageLevel.Increment(0.02f * damage);
            }
        }

        public bool Active() => _activated;

        private void End()
        {
            if (!_activated) return;
            _activated = false;
            _reloadModifier.Remove();
            _fireRateModifier.Remove();
        }

        public void TryStart()
        {
            if (!_rageLevel.ReachedMax() && !_activated) return;
            _activated = true;
            _reloadModifier.Apply();
            _fireRateModifier.Apply();
        }

        public void EnterCombat()
        {
            _rageLevel.SetCurrentValue(0f);
            _reloadModifier.AddTargetAttribute(CombatManager.Player.Weapon().WeaponAttributes.ReloadSpeed);
            _fireRateModifier.AddTargetAttribute(CombatManager.Player.Weapon().WeaponAttributes.FireRate);
        }

        public void ExitCombat()
        {
            _reloadModifier.RemoveTargetAttribute(CombatManager.Player.Weapon().WeaponAttributes.ReloadSpeed);
            _fireRateModifier.RemoveTargetAttribute(CombatManager.Player.Weapon().WeaponAttributes.FireRate);
        }

        public void UpdateCombat()
        {
            if (_activated)
            {
                _rageLevel.Decrement(0.1f * Time.deltaTime);
            }
        }
    }
}