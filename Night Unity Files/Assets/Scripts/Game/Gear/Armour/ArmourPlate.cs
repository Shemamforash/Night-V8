using System;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class ArmourPlate : GearItem
    {
        public readonly bool Inscribable;
        private readonly Number _plateHealth = new Number();
        public const float PlateHealthUnit = 100;
        private bool _broken;

        private ArmourPlate(string name, float weight, ItemQuality itemQuality) : base(name, weight, GearSubtype.Armour, itemQuality)
        {
            if (weight == 5 || weight == 4)
            {
                Inscribable = true;
            }

            _plateHealth.Max = weight * 100;
            _plateHealth.SetCurrentValue(_plateHealth.Max);
        }

        public static ArmourPlate GeneratePlate(ItemQuality plateQuality)
        {
            int weight = (int)plateQuality + 1;
            string name = plateQuality + " Plate";
            return new ArmourPlate(name, weight, plateQuality);
        }

        public override string GetSummary()
        {
            return "+" + Weight + " Armour";
        }

        public void TakeDamage(float amount)
        {
            _plateHealth.Decrement(amount);
            _broken = true;
        }

        public void Repair(float amount)
        {
            _plateHealth.Decrement(amount);
            _broken = false;
        }

        public int GetRepairCost()
        {
            return Mathf.CeilToInt((_plateHealth.Max - _plateHealth.CurrentValue()) / PlateHealthUnit);
        }

        public int GetMaxProtection()
        {
            return Mathf.CeilToInt(_plateHealth.Max / PlateHealthUnit);
        }

        public int GetCurrentProtection()
        {
            return Mathf.CeilToInt(_plateHealth.CurrentValue() / PlateHealthUnit);
        }
        
        public float GetMaxHealth()
        {
            return _plateHealth.Max;
        }

        public float GetRemainingHealth()
        {
            return _plateHealth.CurrentValue();
        }
    }
    
}