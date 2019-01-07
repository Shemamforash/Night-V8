using System.Collections.Generic;
using QuickEngine.Extensions;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class Armour : ResourceItem
    {
        public readonly ItemQuality Quality;
        private static Dictionary<ItemQuality, string> _upgradeRequirements = new Dictionary<ItemQuality, string>();

        public Armour(ResourceTemplate template) : base(template)
        {
            switch (template.Name)
            {
                case "Leather Square":
                    Quality = ItemQuality.Dark;
                    break;
                case "Makeshift Plate":
                    Quality = ItemQuality.Dull;
                    break;
                case "Metal Plate":
                    Quality = ItemQuality.Glowing;
                    break;
                case "Iridescent Scale":
                    Quality = ItemQuality.Shining;
                    break;
                case "Celestial Scale":
                    Quality = ItemQuality.Radiant;
                    break;
            }

            _upgradeRequirements.AddOrUpdate(Quality, template.Name);
        }

        public static string QualityToName(ItemQuality quality)
        {
            Debug.Log(_upgradeRequirements.Keys.Count + " " + quality);
            return _upgradeRequirements[quality];
        }
    }
}