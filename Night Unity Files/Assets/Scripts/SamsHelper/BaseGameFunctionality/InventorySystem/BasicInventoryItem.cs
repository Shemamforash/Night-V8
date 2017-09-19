namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public abstract class BasicInventoryItem
    {
        private readonly string _name;
        private readonly float _weight;
        private readonly ItemType _itemType;
        protected Inventory Inventory;

        public BasicInventoryItem(string name, float weight, ItemType itemType, Inventory inventory = null)
        {
            _name = name;
            _weight = weight;
            _itemType = itemType;
            Inventory = inventory;
        }

        public string Name()
        {
            return _name;
        }

        public virtual string GetItemType()
        {
            return _itemType.ToString();
        }
        
        public virtual string ExtendedName()
        {
            return _name;
        }

        public float Weight()
        {
            return _weight;
        }

        public virtual int Quantity()
        {
            return 1;
        }

        public float TotalWeight()
        {
            return Helper.Round(Weight() * Quantity(), 1);
        }
    }
}