using Facilitating;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    //todo make me a skill cooldown
    public class RageController : ICombatListener
    {
        private readonly Number _rageLevel = new Number(0, 0, 8);
        private bool _activated;

        public RageController()
        {
            _rageLevel.OnMin(End);
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
            RageBarController.SetRageBarFill(_rageLevel.Normalised(), _activated);
        }

        public void Increase(float damage)
        {
            if (!_activated) _rageLevel.Increment(0.02f * damage);
            RageBarController.SetRageBarFill(_rageLevel.Normalised(), _activated);
        }

        private void End()
        {
            if (!_activated) return;
            _activated = false;
        }
    }
}