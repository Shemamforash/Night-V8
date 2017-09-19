using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class Armour : EquippableItem
    {
        protected Armour(string name, float weight) : base(name, weight, GearSlot.Body, ItemType.Armour)
        {
        }

        public override string GetSummary()
        {
            return "Armour";
        }
    }
}