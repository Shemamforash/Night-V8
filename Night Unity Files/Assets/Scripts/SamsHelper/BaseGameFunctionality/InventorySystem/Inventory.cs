using System.Collections.Generic;
using SamsHelper.ReactiveUI;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly Dictionary<string, InventoryItem> _resources = new Dictionary<string, InventoryItem>();
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

        public InventoryItem  GetResource(string resourceName)
        {
            return _resources[resourceName];
        }

        public void AddItem(InventoryItem item)
        {
            if (_items.Count < _inventorySize || _unlimited)
            {
                if (_items.Contains(item))
                {
                    item.Increment(1);
                }
                else
                {
                    _items.Add(item);
                }
            }
        }

        public void AddResource(InventoryItem item)
        {
            _resources[item.Name] = item;
        }

        public void AddLinkedTextToResource(string itemName, ReactiveText<float> reactiveText)
        {
            _resources[itemName].AddLinkedText(reactiveText);
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

        public List<InventoryItem> Items()
        {
            return _items;
        }

        private bool RemoveItem(InventoryItem item)
        {
            if (_items.Contains(item))
            {
                item.Decrement(1);
                if (item.Quantity() == 0)
                {
                    _items.Remove(item);
                }
                return true;
            }
            return false;
        }

        public void MoveItem(InventoryItem item, Inventory other)
        {
            if (RemoveItem(item))
            {
                other.AddItem(item);
            }
        }
    }
}