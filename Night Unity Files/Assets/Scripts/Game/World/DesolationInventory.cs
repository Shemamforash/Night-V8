using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.World
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory(string name) : base(name)
        {
            AddResource("Water", 1);
            AddResource("Food", 1);
            AddResource("Fuel", 1);
            AddResource("Scrap", 0.5f);
            AddResource("Ammo", 0.1f);
        }
    }
}