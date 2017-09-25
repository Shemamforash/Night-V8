using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Gear.Armour
{
    public class Armour : GearItem
    {
        protected Armour(string name, float weight) : base(name, weight, GearSubtype.Armour)
        {
        }

        public override string GetSummary()
        {
            return "Armour";
        }
    }
}