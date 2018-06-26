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
        public  readonly int Protection;

        private ArmourPlate(string name, int protection, ItemQuality quality) : base(name, GearSubtype.Armour, quality)
        {
            Protection = protection;
            _plateHealth.Max = protection * 100;
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

        public override string GetSummary() => "+" + Protection + " Armour";

        public void TakeDamage(float amount)
        {
            _plateHealth.Decrement(amount);
            if (!_plateHealth.ReachedMin()) return;
            Unequip();
            ParentInventory.DestroyItem(this);
        }

        public int GetMaxProtection() => Mathf.CeilToInt(_plateHealth.Max / PlateHealthUnit);

        public int GetCurrentProtection() => Mathf.CeilToInt(_plateHealth.CurrentValue() / PlateHealthUnit);

        public float GetMaxHealth() => _plateHealth.Max;

        public float GetRemainingHealth() => _plateHealth.CurrentValue();
    }
}