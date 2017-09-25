using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class Accessory : GearItem
    {
        protected Accessory(string name, float weight) : base(name, weight, GearSubtype.Accessory)
        {
        }

        public override string GetSummary()
        {
            return "Accessory";
        }
    }
}