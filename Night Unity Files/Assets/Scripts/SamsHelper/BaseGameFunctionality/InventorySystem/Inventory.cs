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
        private readonly List<MyGameObject> _items = new List<MyGameObject>();
        private readonly List<InventoryResource> _resources = new List<InventoryResource>();
        private bool _isWeightLimited;
        private float _maxWeight;

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
            Items().ForEach(i =>
            {
                i.Save(inventoryNode, saveType);
            });
            return inventoryNode;
        }

        public bool IsBottomless()
        {
            return !_isWeightLimited;
        }

        protected List<InventoryResource> InventoryResources()
        {
            return _resources;
        }

        protected List<MyGameObject> Items()
        {
            return _items;
        }

        protected List<MyGameObject> GetItemsOfType(Func<MyGameObject, bool> typeCheck)
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

        public bool ContainsItem(MyGameObject item)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource == null) return _items.Contains(item);
            return _resources.Contains(resource) && resource.Quantity() != 0;
        }

        protected virtual void AddItem(MyGameObject item)
        {
            Weight += item.Weight;
            item.ParentInventory = this;
            _items.Add(item);
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        public virtual MyGameObject RemoveItem(MyGameObject item)
        {
            if (!_items.Contains(item)) throw new Exceptions.ItemNotInInventoryException(item.Name);
            _items.Remove(item);
            Weight -= item.Weight;
            return item;
        }

        protected void AddResource(InventoryResourceType type, float weight)
        {
            if (_resources.FirstOrDefault(r => r.GetResourceType() == type) != null) throw new Exceptions.ResourceAlreadyExistsException(type.ToString());
            _resources.Add(new InventoryResource(type, weight));
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
            return true;
        }

        public float GetResourceQuantity(InventoryResourceType type)
        {
            return GetResource(type).Quantity();
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
            MyGameObject movedItem;
            if (!target.InventoryHasSpace(item.Weight)) return null;
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

            return movedItem;
        }

        public MyGameObject Move(MyGameObject item, Inventory target, float quantity)
        {
            InventoryResource resource = item as InventoryResource;
            if (resource != null)
            {
                if (quantity > resource.Quantity()) quantity = resource.Quantity();
                if (!target.InventoryHasSpace(resource.GetWeight(quantity)))
                {
                    float remainingSpace = target.MaxWeight - target.Weight;
                    quantity = (int) Math.Floor(remainingSpace / resource.Weight);
                }

                if (!(quantity > 0)) return null;
                DecrementResource(resource, quantity);
                InventoryResource targetResource = target.GetResource(resource.GetResourceType());
                target.IncrementResource(targetResource, quantity);
                return targetResource;
            }

            return Move(item, target);
        }

        public void MoveAllResources(Inventory target)
        {
            foreach (InventoryResource resource in _resources) Move(resource, target, resource.Quantity());
        }

        private void LoadResource(InventoryResourceType type, XmlNode root)
        {
            IncrementResource(type, SaveController.ParseIntFromNodeAndString(root, type.ToString()));
        }

        private void SaveResource(InventoryResourceType type, XmlNode root)
        {
            SaveController.CreateNodeAndAppend(type.ToString(), root, GetResourceQuantity(type));
        }

        public virtual List<MyGameObject> SortByType()
        {
            throw new NotImplementedException();
        }
    }
}