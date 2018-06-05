namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceTemplate
    {
        public readonly string Name, Environment, Region, DropLocation, Effect1, Effect2;
        public readonly float Duration1, Duration2, Weight;
        private readonly bool Consumable;

        public ResourceTemplate(string name, float weight, string environment, string region, string dropLocation, string effect1, string effect2, float duration1, float duration2) : this(name, weight,
            environment, region,
            dropLocation)
        {
            Effect1 = effect1;
            Effect2 = effect2;
            Duration1 = duration1;
            Duration2 = duration2;
            Consumable = true;
        }

        public ResourceTemplate(string name, float weight, string environment, string region, string dropLocation)
        {
            Name = name;
            Weight = weight;
            Environment = environment;
            Region = region;
            DropLocation = dropLocation;
        }

        public InventoryItem Create()
        {
            InventoryItem item = Consumable ? new Consumable(this, GameObjectType.Resource) : new InventoryItem(this, GameObjectType.Resource);
            item.SetStackable(true);
            return item;
        }
    }
}