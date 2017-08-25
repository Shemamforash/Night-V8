using System.Collections.Generic;
using SamsHelper;
using SamsHelper.ReactiveUI;

namespace Game.World
{
    public class Inventory
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();
        private readonly int _inventorySize;
        private readonly bool _unlimited;

        public Inventory()
        {
            _unlimited = true;
        }

        public Inventory(int inventorySize)
        {
            _inventorySize = inventorySize;
            _unlimited = false;
        }

        public Resource  GetResource(string resourceName)
        {
            return _resources[resourceName];
        }

        public void AddItem(InventoryItem item)
        {
            if (_items.Count < _inventorySize || _unlimited)
            {
                _items.Add(item);
            }
        }

        public void AddResource(string name, float min, float max)
        {
            _resources[name] = new Resource(name, min, max);
        }

        public void AddResource(string name, ReactiveText<float> reactiveText)
        {
            _resources[name] = new Resource(name);
            _resources[name].AddLinkedText(reactiveText);
        }

        public void IncrementResource(string name, float amount)
        {
            if (amount < 0)
            {
                throw new Exceptions.ResourceValueChangeInvalid(name, "increment", amount);
            }
            _resources[name].Increment(amount);
        }

        public float DecrementResource(string name, float amount)
        {
            if (amount < 0)
            {
                throw new Exceptions.ResourceValueChangeInvalid(name, "decrement", amount);
            }
            return _resources[name].Decrement(amount);
        }

        public float GetResourceQuantity(string name)
        {
            return _resources[name].Quantity();
        }
    }
}