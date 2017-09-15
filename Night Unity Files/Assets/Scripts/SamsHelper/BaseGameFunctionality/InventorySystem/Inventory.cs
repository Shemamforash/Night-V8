﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory
    {
        private readonly List<InventoryResource> _resources = new List<InventoryResource>();
        private readonly List<BasicInventoryItem> _items = new List<BasicInventoryItem>();
        private bool _isWeightLimited;
        private float _inventoryWeight;
        private float _maxWeight;
        private readonly string _name;
        
        public Inventory(string name)
        {
            _name = name;
            MaxWeight = float.MaxValue;
        }

        public Inventory(string name, float maxWeight)
        {
            _name = name;
            MaxWeight = maxWeight;
            _isWeightLimited = true;
        }

        public List<InventoryResource> Resources()
        {
            return _resources;
        }

        public string Name()
        {
            return _name;
        }
        
        public List<BasicInventoryItem> Items()
        {
            return _items;
        }
        
        public float MaxWeight
        {
            get { return _maxWeight; }
            set
            {
                _maxWeight = value;
                _isWeightLimited = true;
            }
        }

        public InventoryResource GetResource(string resourceName)
        {
            InventoryResource found = _resources.FirstOrDefault(item => item.Name() == resourceName);
            if (found == null)
            {
                throw new Exceptions.ResourceDoesNotExistException(resourceName);
            }
            return found;
        }

        public bool InventoryHasSpace(float weight)
        {
            if (_inventoryWeight + weight > MaxWeight + 0.0001f && _isWeightLimited)
            {
                return false;
            }
            return true;
        }

        public float GetInventoryWeight()
        {
            return _inventoryWeight;
        }

        public bool ContainsItem(BasicInventoryItem item)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource == null) return _items.Contains(item);
            return _resources.Contains(resource) && resource.Quantity() != 0;
        }

        //Returns true if new instance of item was added
        //Returns false if existing instance was incremented
        public void AddItem(BasicInventoryItem item)
        {
            _inventoryWeight += item.Weight();
            _items.Add(item);
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        private BasicInventoryItem RemoveItem(BasicInventoryItem item)
        {
            if (!_items.Contains(item))
            {
                throw new Exceptions.ItemNotInInventoryException(item.Name());
            }
            _items.Remove(item);
            _inventoryWeight -= item.Weight();
            return item;
        }

        public void AddResource(string name, float weight)
        {
            if (_resources.FirstOrDefault(r => r.Name() == name) != null)
            {
                throw new Exceptions.ResourceAlreadyExistsException(name);
            }
            _resources.Add(new InventoryResource(name, weight));
        }

        public void IncrementResource(string name, int amount)
        {
            IncrementResource(GetResource(name), amount);
        }

        public void IncrementResource(InventoryResource resource, int amount)
        {
            if (amount < 0)
            {
                throw new Exceptions.ResourceValueChangeInvalid(resource.Name(), "increment", amount);
            }
            _inventoryWeight += resource.GetWeight(amount);
            resource.Increment(amount);
        }

        public int DecrementResource(string name, int amount)
        {
            return DecrementResource(GetResource(name), amount);
        }

        public int DecrementResource(InventoryResource resource, int amount)
        {
            if (amount < 0)
            {
                throw new Exceptions.ResourceValueChangeInvalid(resource.Name(), "decrement", amount);
            }
            _inventoryWeight -= resource.GetWeight(amount);
            return resource.Decrement(amount);
        }

        public int GetResourceQuantity(string name)
        {
            return GetResource(name).Quantity();
        }

        public List<BasicInventoryItem> Contents()
        {
            List<BasicInventoryItem> contents = new List<BasicInventoryItem>();
            _items.ForEach(i => contents.Add(i));
            _resources.ForEach(r => contents.Add(r));
            return contents;
        }

        //Returns item in target inventory if the item was successfully moved
        private BasicInventoryItem Move(BasicInventoryItem item, Inventory target)
        {
            BasicInventoryItem movedItem = null;
            if (target.InventoryHasSpace(item.Weight()))
            {
                InventoryResource resource = item as InventoryResource;
                if (resource == null)
                {
                    movedItem = RemoveItem(item);
                    target.AddItem(movedItem);
                }
                else
                {
                    movedItem = Move(resource, target, 1);
                }
            }
            return movedItem;
        }

        //BUG creates new resources when moving resources to fill remainder of inventory
        public BasicInventoryItem Move(BasicInventoryItem item, Inventory target, int quantity)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource != null)
            {
                if (!target.InventoryHasSpace(resource.GetWeight(quantity)))
                {
                    float remainingSpace = target.MaxWeight - target._inventoryWeight;
                    quantity = (int) Math.Floor(remainingSpace / resource.Weight());
                }
                if (quantity != 0)
                {
                    DecrementResource(resource, quantity);
                    InventoryResource targetResource = target.GetResource(resource.Name());
                    target.IncrementResource(targetResource, quantity);
                    return targetResource;
                }
            }
            else
            {
                return Move(item, target);
            }
            return null;
        }

        public void MoveAllResources(Inventory target)
        {
            foreach (InventoryResource resource in _resources)
            {
                Move(resource, target, resource.Quantity());
            }
        }
    }
}