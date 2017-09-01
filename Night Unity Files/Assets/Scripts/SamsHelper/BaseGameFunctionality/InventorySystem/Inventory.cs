using System.Collections.Generic;
using System.Linq;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly int _inventorySize;
        private readonly bool _unlimitedSize, _isWeightLimited;
        private float _inventoryWeight;
        private float _maxWeight;

        public Inventory()
        {
            _unlimitedSize = true;
            _maxWeight = float.MaxValue;
        }

        public Inventory(int inventorySize)
        {
            _inventorySize = inventorySize;
            _unlimitedSize = false;
        }

        public Inventory(float maxWeight)
        {
            _maxWeight = maxWeight;
            _isWeightLimited = true;
        }

        public InventoryItem GetResource(string resourceName)
        {
            InventoryItem found = _items.First(item => item.Name == resourceName);
            if (found == null)
            {
                throw new Exceptions.ResourceDoesNotExistException(resourceName);
            }
            return found;
        }

        public void SetMaxWeight(float maxWeight)
        {
            _maxWeight = maxWeight;
        }

        public bool InventoryHasSpace(InventoryItem item)
        {
            if (_items.Count < _inventorySize || _unlimitedSize)
            {
                return !_isWeightLimited || !(_inventoryWeight >= _maxWeight);
            }
            return false;
        }

        private InventoryItem Contains(string itemName)
        {
            return _items.FirstOrDefault(item => item.Name == itemName);
        }
        
        //Returns true if new instance of item was added
        //Returns false if existing instance was incremented
        public InventoryItem AddItem(InventoryItem item)
        {
            _inventoryWeight += item.GetWeight(1);
            InventoryItem foundItem = Contains(item.Name);
            if (foundItem != null && item.Stackable())
            {
                foundItem.Increment(item.Quantity());
                return foundItem;
            }
            _items.Add(item);
            return item;
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        private InventoryItem RemoveItem(InventoryItem item)
        {
            if (!_items.Contains(item))
            {
                throw new Exceptions.ItemNotInInventoryException(item.Name);
            }
            if (item.Quantity() <= 1)
            {
                if (item.DestroyOnEmpty())
                {
                    _items.Remove(item);
                }
                else
                {
                    item.Decrement(1);
                }
                return item;
            }
            if (item.Quantity() > 1)
            {
                item.Decrement(1);
                _inventoryWeight -= item.GetWeight(1);
                return item.Clone();
            }
            return null;
        }

        public void AddResource(InventoryItem item)
        {
            AddItem(item);
            item.AllowStacking();
            item.DontDestroyOnEmpty();
        }

        public void AddLinkedTextToResource(string itemName, ReactiveText<float> reactiveText)
        {
            GetResource(itemName).AddLinkedText(reactiveText);
        }

        public void IncrementResource(string name, float amount)
        {
            if (amount < 0)
            {
                throw new Exceptions.ResourceValueChangeInvalid(name, "increment", amount);
            }
            GetResource(name).Increment(amount);
        }

        public float DecrementResource(string name, float amount)
        {
            if (amount < 0)
            {
                throw new Exceptions.ResourceValueChangeInvalid(name, "decrement", amount);
            }
            return GetResource(name).Decrement(amount);
        }

        public float GetResourceQuantity(string name)
        {
            return GetResource(name).Quantity();
        }

        public List<InventoryItem> Items()
        {
            return _items;
        }

        private bool ContainsAtLeastOne(string itemName)
        {
            InventoryItem itemInInventory = Items().First(item => item.Name == itemName && item.Quantity() > 0);
            return itemInInventory != null;
        }

        //Returns item in target inventory if the item was successfully moved
        public InventoryItem MoveItem(InventoryItem item, Inventory target)
        {
            if (this == target)
            {
                throw new Exceptions.MoveItemToSameInventoryException();
            }
            if (target.InventoryHasSpace(item) && ContainsAtLeastOne(item.Name))
            {
                InventoryItem removedItem = RemoveItem(item);
                return target.AddItem(removedItem);
            }
            return null;
        }
    }
}