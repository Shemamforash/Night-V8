using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.World
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory()
        {
            AddResource(new InventoryItem("Water", 1));
            AddResource(new InventoryItem("Food", 1));
            AddResource(new InventoryItem("Fuel", 1));
            AddResource(new InventoryItem("Scrap", 0.5f));
            AddResource(new InventoryItem("Ammo", 0.1f));
        }
    }
}