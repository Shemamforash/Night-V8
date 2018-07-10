using System;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class ArmourPlate : GearItem
    {
        public const float PlateHealthUnit = 100;
        private readonly Number _plateHealth = new Number();
        public readonly int Protection;

        private ArmourPlate(string name, ItemQuality quality) : base(name, GearSubtype.Armour, quality)
        {
            Protection = (int) quality + 1;
            _plateHealth.Max = Protection * PlateHealthUnit;
            _plateHealth.SetCurrentValue(_plateHealth.Max);
        }

        private static string QualityToName(ItemQuality quality)
        {
            switch (quality)
            {
                case ItemQuality.Rusted:
                    return "Leather Plate";
                case ItemQuality.Worn:
                    return "Reinforced Leather Plate";
                case ItemQuality.Shining:
                    return "Metal Plate";
                case ItemQuality.Faultless:
                    return "Alloy Plate";
                case ItemQuality.Radiant:
                    return "Living Metal Plate";
            }

            throw new ArgumentOutOfRangeException("Unknown armour type '" + quality + "'");
        }

        public static ArmourPlate Create(ItemQuality quality)
        {
            string name = QualityToName(quality);
            return new ArmourPlate(name, quality);
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
    }
}