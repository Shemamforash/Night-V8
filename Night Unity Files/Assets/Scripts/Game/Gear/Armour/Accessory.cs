using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class Accessory : EquippableItem
    {
        protected Accessory(string name, float weight) : base(name, weight, GearSlot.Accessory, ItemType.Accessory)
        {
        }

        public override string GetSummary()
        {
            return "Accessory";
        }
    }
}