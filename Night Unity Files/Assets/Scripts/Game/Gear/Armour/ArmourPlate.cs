using System;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class ArmourPlate : GearItem
    {
        public const float PlateHealthUnit = 100;
        private readonly Number _plateHealth = new Number();
        public readonly bool Inscribable;
        private bool _broken;

        private ArmourPlate(string name, float weight, ItemQuality quality) : base(name, weight, GearSubtype.Armour, quality)
        {
            if (weight == 5 || weight == 4) Inscribable = true;

            _plateHealth.Max = weight * 100;
            _plateHealth.SetCurrentValue(_plateHealth.Max);
        }

        private static ItemQuality NameToQuality(string name)
        {
            switch (name)
            {
                case "Leather Plate":
                    return ItemQuality.Rusted;
                case "Reinforced Leather Plate":
                    return ItemQuality.Worn;
                case "Metal Plate":
                    return ItemQuality.Shining;
                case "Alloy Plate":
                    return ItemQuality.Faultless;
                case "Living Metal Plate":
                    return ItemQuality.Radiant;
            }

            throw new ArgumentOutOfRangeException("Unknown armour type '" + name + "'");
        }


        public static ArmourPlate Create(string plateName)
        {
            ItemQuality quality = NameToQuality(plateName);
            int weight = (int) quality + 1;
            return new ArmourPlate(plateName, weight, quality);
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