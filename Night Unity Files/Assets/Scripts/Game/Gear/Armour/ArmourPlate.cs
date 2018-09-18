using System;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class ArmourPlate : GearItem
    {
        public const float PlateHealthUnit = 200;
        private readonly Number _plateHealth = new Number();
        public readonly int Protection;

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("Health", _plateHealth.CurrentValue());
            return doc;
        }
        
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
                case ItemQuality.Dark:
                    return "Leather Plate";
                case ItemQuality.Dull:
                    return "Reinforced Leather Plate";
                case ItemQuality.Glowing:
                    return "Metal Plate";
                case ItemQuality.Shining:
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
            ParentInventory().DestroyItem(this);
        }

        public int GetMaxProtection() => Mathf.CeilToInt(_plateHealth.Max / PlateHealthUnit);

        public int GetCurrentProtection() => Mathf.CeilToInt(_plateHealth.CurrentValue() / PlateHealthUnit);

        public static ArmourPlate LoadArmour(XmlNode root)
        {
            ItemQuality quality = (ItemQuality) root.IntFromNode("Quality");
            ArmourPlate plate = Create(quality);
            plate.Load(root);
            plate._plateHealth.SetCurrentValue(root.FloatFromNode("Health"));
            return plate;
        }
    }
}