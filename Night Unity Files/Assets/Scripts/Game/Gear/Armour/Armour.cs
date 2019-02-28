using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class Armour : ResourceItem
    {
        private static Dictionary<ItemQuality, string> _upgradeRequirements;

        public Armour(ResourceTemplate template) : base(template)
        {
        }

        private static void CreateUpgradeRequirements()
        {
            if (_upgradeRequirements != null) return;
            _upgradeRequirements = new Dictionary<ItemQuality, string>();
            _upgradeRequirements.Add(ItemQuality.Dark, "Leather Square");
            _upgradeRequirements.Add(ItemQuality.Dull, "Makeshift Plate");
            _upgradeRequirements.Add(ItemQuality.Glowing, "Metal Plate");
            _upgradeRequirements.Add(ItemQuality.Shining, "Iridescent Scale");
            _upgradeRequirements.Add(ItemQuality.Radiant, "Celestial Scale");
        }

        public static string QualityToName(ItemQuality quality)
        {
            CreateUpgradeRequirements();
            return _upgradeRequirements[quality];
        }
    }
}