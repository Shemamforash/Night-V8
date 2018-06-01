using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory : MyGameObject, IPersistenceTemplate
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly List<InventoryResource> _resources = new List<InventoryResource>();
        private bool _isWeightLimited;
        private float _maxWeight;
        private readonly List<InventoryItem> _contents = new List<InventoryItem>();
        private bool _readonly;

        protected Inventory(string name, float maxWeight = 0) : base(name, GameObjectType.Inventory)
        {
            if (maxWeight == 0) return;
            MaxWeight = maxWeight;
            _isWeightLimited = true;
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

        public virtual void Load(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return;
            XmlNode inventoryNode = root.SelectSingleNode(Name);
            InventoryResources().ForEach(r => LoadResource(r.GetResourceType(), inventoryNode));
        }

        public virtual XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            root = base.Save(root, saveType);
            XmlNode inventoryNode = SaveController.CreateNodeAndAppend("Inventory", root);
            SaveController.CreateNodeAndAppend("Name", inventoryNode, Name);
            InventoryResources().ForEach(r => SaveResource(r.GetResourceType(), inventoryNode));
            Items().ForEach(i => { i.Save(inventoryNode, saveType); });
            return inventoryNode;
        }

        public void SetReadonly(bool readOnly)
        {
            _readonly = readOnly;
        }
        
        public bool IsBottomless()
        {
            return !_isWeightLimited;
        }

        protected List<InventoryResource> InventoryResources()
        {
            return _resources;
        }

        protected List<InventoryItem> Items()
        {
            return _items;
        }

        protected List<InventoryItem> GetItemsOfType(Func<InventoryItem, bool> typeCheck)
        {
            return _items.Where(typeCheck).ToList();
        }

        public InventoryResource GetResource(InventoryResourceType resourceType)
        {
            InventoryResource found = _resources.FirstOrDefault(item => item.GetResourceType() == resourceType);
            if (found == null) throw new Exceptions.ResourceDoesNotExistException(resourceType.ToString());
            return found;
        }

        private bool InventoryHasSpace(float weight)
        {
            return !(Weight + weight > MaxWeight + 0.0001f) || !_isWeightLimited;
        }

        public bool ContainsItem(InventoryItem item)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource == null) return _items.Contains(item);
            return _resources.Contains(resource) && resource.Quantity() != 0;
        }

        protected virtual void AddItem(InventoryItem item)
        {
            Weight += item.Weight;
            item.ParentInventory = this;
            _items.Add(item);
            UpdateContents();
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        protected virtual InventoryItem RemoveItem(InventoryItem item)
        {
            if (!_items.Contains(item)) throw new Exceptions.ItemNotInInventoryException(item.Name);
            _items.Remove(item);
            Weight -= item.Weight;
            UpdateContents();
            return item;
        }

        protected void AddResource(InventoryResourceType type, float weight)
        {
            if (_resources.FirstOrDefault(r => r.GetResourceType() == type) != null) throw new Exceptions.ResourceAlreadyExistsException(type.ToString());
            InventoryResource newResource = new InventoryResource(type, weight);
            newResource.ParentInventory = this;
            _resources.Add(newResource);
        }

        public void IncrementResource(InventoryResourceType type, float amount)
        {
            IncrementResource(GetResource(type), amount);
        }

        private void IncrementResource(InventoryResource resource, float amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "increment", amount);
            Weight += resource.GetWeight(amount);
            resource.Increment(amount);
            UpdateContents();
        }

        public bool DecrementResource(InventoryResourceType type, float amount)
        {
            return DecrementResource(GetResource(type), amount);
        }

        private bool DecrementResource(InventoryResource resource, float amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "decrement", amount);
            if (resource.Quantity() < amount) return false;
            Weight -= resource.GetWeight(amount);
            resource.Decrement(amount);
            UpdateContents();
            return true;
        }

        public float GetResourceQuantity(InventoryResourceType type)
        {
            return GetResource(type).Quantity();
        }

        private void UpdateContents()
        {
            _contents.Clear();
            _items.ForEach(i => _contents.Add(i));
            _resources.ForEach(r =>
            {
                if (r.Quantity() == 0) return;
                _contents.Add(r);
            });
        }
        
        public List<InventoryItem> Contents()
        {
            return _contents;
        }

        //Returns item in target inventory if the item was successfully moved
        private InventoryItem Move(InventoryItem item)
        {
            if (InventoryHasSpace(item.Weight)) return null;
            Inventory parent = item.ParentInventory;
            InventoryItem movedItem = parent == null ? item : parent.RemoveItem(item);
            AddItem(movedItem);
            return movedItem;
        }

        public InventoryItem Move(InventoryItem item, float quantity)
        {
            if (_readonly) return null;
            InventoryResource resource = item as InventoryResource;
            if (resource == null) return Move(item);
            if (quantity > resource.Quantity()) quantity = resource.Quantity();
            if (!InventoryHasSpace(resource.GetWeight(quantity)))
            {
                float remainingSpace = MaxWeight - Weight;
                quantity = (int) Math.Floor(remainingSpace / resource.Weight);
            }

            if (quantity <= 0) return null;
            item.ParentInventory?.DecrementResource(resource, quantity);
            InventoryResource targetResource = GetResource(resource.GetResourceType());
            IncrementResource(targetResource, quantity);
            return targetResource;
        }

        public void MoveAllResources(Inventory target)
        {
            foreach (InventoryResource resource in _resources) target.Move(resource, resource.Quantity());
        }

        private void LoadResource(InventoryResourceType type, XmlNode root)
        {
            IncrementResource(type, SaveController.ParseIntFromNodeAndString(root, type.ToString()));
        }

        private void SaveResource(InventoryResourceType type, XmlNode root)
        {
            SaveController.CreateNodeAndAppend(type.ToString(), root, GetResourceQuantity(type));
        }

        public virtual List<InventoryItem> SortByType()
        {
            throw new NotImplementedException();
        }
    }
}