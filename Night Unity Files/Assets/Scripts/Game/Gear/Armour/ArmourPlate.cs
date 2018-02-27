using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class ArmourPlate : GearItem
    {
        public readonly bool _inscribable;

        private ArmourPlate(string name, float weight, ItemQuality itemQuality) : base(name, weight, GearSubtype.Armour, itemQuality)
        {
            if (weight == 5)
            {
                _inscribable = true;
            }
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
    }
    
}