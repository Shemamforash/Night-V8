using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.World
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory(string name) : base(name)
        {
            AddResource(InventoryResourceType.Water, 1);
            AddResource(InventoryResourceType.Food, 1);
            AddResource(InventoryResourceType.Fuel, 1);
            AddResource(InventoryResourceType.Scrap, 0.5f);
            AddResource(InventoryResourceType.Ammo, 0.1f);
        }
    }
}