using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat
{
    public class RecoilManager : ICombatListener 
    {
        public readonly Number Recoil = new Number(0, 0, 1f);
        private const float RecoilRecoveryRate = 0.02f;
        private const float TimeToStartRecovery = 0.5f;
        private float _recoveryTimer;

        public void EnterCombat()
        {
            Recoil.SetCurrentValue(0);
        }

        public void ExitCombat()
        {
        }

        public void UpdateCombat()
        {
            if (_recoveryTimer > 0)
            {
                _recoveryTimer -= Time.deltaTime;
                return;
            }
            Recoil.Decrement(RecoilRecoveryRate + Time.deltaTime);
        }

        public void Increment(Weapon w)
        {
            float recoilLoss = w.GetAttributeValue(AttributeType.Handling);
            recoilLoss = 100 - recoilLoss;
            recoilLoss /= 100;
            Recoil.Increment(recoilLoss);
            _recoveryTimer = TimeToStartRecovery;
        }

        public float GetAccuracyModifier()
        {
            return -0.5f * Recoil.CurrentValue() + 1;
        }
    }
}