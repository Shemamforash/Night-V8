using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory : MyGameObject, IPersistenceTemplate
    {
        private readonly List<InventoryResource> _resources = new List<InventoryResource>();
        private readonly List<MyGameObject> _items = new List<MyGameObject>();
        private bool _isWeightLimited;
        private float _maxWeight;
        
        public Inventory(string name, float maxWeight = 0) : base(name, GameObjectType.Inventory)
        {
            if (maxWeight != 0)
            {
                MaxWeight = maxWeight;
                _isWeightLimited = true;
            }
        }

        public List<InventoryResource> Resources()
        {
            return _resources;
        }

        public List<MyGameObject> Items()
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
            InventoryResource found = _resources.FirstOrDefault(item => item.Name == resourceName);
            if (found == null)
            {
                throw new Exceptions.ResourceDoesNotExistException(resourceName);
            }
            return found;
        }

        public bool InventoryHasSpace(float weight)
        {
            if (Weight + weight > MaxWeight + 0.0001f && _isWeightLimited)
            {
                return false;
            }
            return true;
        }

        public bool ContainsItem(MyGameObject item)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource == null) return _items.Contains(item);
            return _resources.Contains(resource) && resource.Quantity() != 0;
        }

        //Returns true if new instance of item was added
        //Returns false if existing instance was incremented
        public virtual void AddItem(MyGameObject item)
        {
            Weight += item.Weight;
            _items.Add(item);
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        public virtual MyGameObject RemoveItem(MyGameObject item)
        {
            if (!_items.Contains(item))
            {
                throw new Exceptions.ItemNotInInventoryException(item.Name);
            }
            _items.Remove(item);
            Weight -= item.Weight;
            return item;
        }

        public void AddResource(string name, float weight)
        {
            if (_resources.FirstOrDefault(r => r.Name == name) != null)
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
                throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "increment", amount);
            }
            Weight += resource.GetWeight(amount);
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
                throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "decrement", amount);
            }
            Weight -= resource.GetWeight(amount);
            return resource.Decrement(amount);
        }

        public int GetResourceQuantity(string name)
        {
            return GetResource(name).Quantity();
        }

        public List<MyGameObject> Contents()
        {
            List<MyGameObject> contents = new List<MyGameObject>();
            _items.ForEach(i => contents.Add(i));
            _resources.ForEach(r => contents.Add(r));
            return contents;
        }

        //Returns item in target inventory if the item was successfully moved
        private MyGameObject Move(MyGameObject item, Inventory target)
        {
            MyGameObject movedItem = null;
            if (target.InventoryHasSpace(item.Weight))
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
        public MyGameObject Move(MyGameObject item, Inventory target, int quantity)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource != null)
            {
                if (!target.InventoryHasSpace(resource.GetWeight(quantity)))
                {
                    float remainingSpace = target.MaxWeight - target.Weight;
                    quantity = (int) Math.Floor(remainingSpace / resource.Weight);
                }
                if (quantity != 0)
                {
                    DecrementResource(resource, quantity);
                    InventoryResource targetResource = target.GetResource(resource.Name);
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
        
        public void Load(XmlNode root, PersistenceType saveType)
        {
            if (saveType == PersistenceType.Game)
            {
                XmlNode inventoryNode = root.SelectSingleNode(Name);
                Resources().ForEach(r => LoadResource(r.Name, inventoryNode));
            }
        }

        public void Save(XmlNode root, PersistenceType saveType)
        {
            if (saveType == PersistenceType.Game)
            {
                XmlNode inventoryNode = SaveController.CreateNodeAndAppend(Name, root);
                Resources().ForEach(r => SaveResource(r.Name, inventoryNode));
            }
        }

        private void LoadResource(string resourceName, XmlNode root)
        {
            IncrementResource(resourceName, SaveController.ParseIntFromNodeAndString(root, resourceName));
        }
	    
        private void SaveResource(string resourceName, XmlNode root)
        {
            SaveController.CreateNodeAndAppend(resourceName, root, GetResourceQuantity(resourceName));
        }
    }
}