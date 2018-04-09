using Facilitating;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class RageController : ICombatListener
    {
        private readonly AttributeModifier _fireRateModifier = new AttributeModifier(AttributeType.FireRate);
        private readonly Number _rageLevel = new Number(0, 0, 8);
        private readonly AttributeModifier _reloadModifier = new AttributeModifier(AttributeType.ReloadSpeed);
        private bool _activated;


        public RageController()
        {
            _reloadModifier.SetMultiplicative(0.5f);
            _fireRateModifier.SetMultiplicative(2f);
            _rageLevel.OnMin(End);
            _rageLevel.AddOnValueChange(a => RageBarController.SetRageBarFill(a.Normalised(), _activated));
        }

        public void EnterCombat()
        {
            _rageLevel.SetCurrentValue(0f);
        }

        public void ExitCombat()
        {
            End();
        }

        public void UpdateCombat()
        {
            if (_activated) _rageLevel.Decrement(0.1f * Time.deltaTime);
        }

        public float CurrentValue()
        {
            return _rageLevel.CurrentValue();
        }

        public void Increase(float damage)
        {
            if (!_activated) _rageLevel.Increment(0.02f * damage);
        }

        public bool Active()
        {
            return _activated;
        }

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
            _reloadModifier.Apply(CombatManager.Player.Weapon().WeaponAttributes);
            _fireRateModifier.Apply(CombatManager.Player.Weapon().WeaponAttributes);
        }
    }
}