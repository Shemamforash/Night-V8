namespace Game.World
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory()
        {
            AddResource("Water", 0, float.MaxValue);
            AddResource("Food", 0, float.MaxValue);
            AddResource("Fuel", 0, float.MaxValue);
            AddResource("Scrap", 0, float.MaxValue);
            AddResource("Ammo", 0, float.MaxValue);
        }
    }
}