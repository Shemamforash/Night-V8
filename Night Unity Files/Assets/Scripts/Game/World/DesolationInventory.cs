using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.World
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory()
        {
            AddResource(new InventoryItem("Water", 1, 0, float.MaxValue));
            AddResource(new InventoryItem("Food", 1, 0, float.MaxValue));
            AddResource(new InventoryItem("Fuel", 1, 0, float.MaxValue));
            AddResource(new InventoryItem("Scrap", 0.5f, 0, float.MaxValue));
            AddResource(new InventoryItem("Ammo", 0.1f, 0, float.MaxValue));
        }
    }
}