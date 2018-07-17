using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;
using UnityEngine.Assertions;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory : MyGameObject, IPersistenceTemplate
    {
        private readonly Dictionary<string, InventoryItem> _resources = new Dictionary<string, InventoryItem>();
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly List<InventoryItem> _contents = new List<InventoryItem>();
        private readonly List<Consumable> _consumables = new List<Consumable>();
        private static bool _loaded;
        private static List<AttributeType> _attributeTypes;

        public static AttributeType StringToAttributeType(string attributeString)
        {
            if (_attributeTypes == null)
            {
                _attributeTypes = new List<AttributeType>();
                foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType))) _attributeTypes.Add(attributeType);
            }

            return _attributeTypes.Find(t => t.ToString() == attributeString);
        }
        
        public Inventory(string name) : base(name, GameObjectType.Inventory)
        {
            LoadResources();
        }

        public List<Consumable> Consumables()
        {
            return _consumables.Where(c => c.Quantity() != 0).ToList();
        }

        private static void LoadResources()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Resources");

            foreach (XmlNode resourceNode in root.SelectNodes("Consumable"))
                new ResourceTemplate(resourceNode);

            foreach (XmlNode resourceNode in root.SelectNodes("Resource"))
                new ResourceTemplate(resourceNode);

            _loaded = true;
        }

        public void Print()
        {
            string contents = "Resources: \n\n";
            foreach (InventoryItem r in InventoryResources())
            {
                contents += r.Name + " x" + r.Quantity() + "\n";
            }

            contents += "Items: \n\n";
            foreach (InventoryItem item in Items())
            {
                contents += item.Name + " x" + item.Quantity() + "\n";
            }

            Debug.Log(contents);
        }

        public virtual void Load(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return;
            XmlNode inventoryNode = root.SelectSingleNode(Name);
            InventoryResources().ForEach(r => LoadResource(r.Name, inventoryNode));
        }

        public virtual XmlNode Save(XmlNode root, PersistenceType saveType)
        {
            if (saveType != PersistenceType.Game) return null;
            root = base.Save(root, saveType);
            XmlNode inventoryNode = SaveController.CreateNodeAndAppend("Inventory", root);
            SaveController.CreateNodeAndAppend("Name", inventoryNode, Name);
            InventoryResources().ForEach(r => SaveResource(r.Name, inventoryNode));
            Items().ForEach(i => { i.Save(inventoryNode, saveType); });
            return inventoryNode;
        }

        private List<InventoryItem> InventoryResources()
        {
            return _resources.Values.ToList();
        }

        protected List<InventoryItem> Items()
        {
            return _items;
        }

        private InventoryItem AddResource(string name)
        {
            ResourceTemplate template = ResourceTemplate.AllResources.FirstOrDefault(t => t.Name == name);
            if (template == null) throw new Exceptions.ResourceDoesNotExistException(template.Name);
            InventoryItem newResource = template.Create();
            newResource.ParentInventory = this;
            if (newResource is Consumable)
            {
                _consumables.Add((Consumable) newResource);
            }

            _resources.Add(name, newResource);
            return newResource;
        }

        private InventoryItem GetResource(string resourceName)
        {
            InventoryItem item;
            _resources.TryGetValue(resourceName, out item);
            return item;
        }

        public bool ContainsItem(InventoryItem item)
        {
            if (!item.IsStackable()) return _items.Contains(item);
            return _resources.ContainsKey(item.Name) && item.Quantity() != 0;
        }

        protected virtual void AddItem(InventoryItem item)
        {
            item.ParentInventory = this;
            _items.Add(item);
            Assert.IsFalse(item is Consumable);
            UpdateContents();
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        protected virtual InventoryItem RemoveItem(InventoryItem item)
        {
            if (!_contents.Contains(item)) throw new Exceptions.ItemNotInInventoryException(item.Name);
            _items.Remove(item);
            if (item is Consumable) _consumables.Remove((Consumable) item);
            _resources.Remove(item.Name);
            UpdateContents();
            return item;
        }

        public void IncrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "increment", amount);
            InventoryItem resource = GetResource(name);
            if (resource == null) resource = AddResource(name);
            resource.Increment(amount);
            UpdateContents();
        }

        public void DecrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "decrement", amount);
            InventoryItem resource = GetResource(name);
            if (resource == null) throw new Exceptions.ResourceDoesNotExistException(name);
            if (resource.Quantity() < amount) return;
            resource.Decrement(amount);
            if (resource.Quantity() == 0) RemoveItem(resource);
            UpdateContents();
        }

        public int GetResourceQuantity(string type)
        {
            return GetResource(type)?.Quantity() ?? 0;
        }

        private void UpdateContents()
        {
            _contents.Clear();
            _items.ForEach(i => _contents.Add(i));
            foreach (InventoryItem r in _resources.Values)
            {
                Assert.IsTrue(r.Quantity() > 0);
                _contents.Add(r);
            }
        }

        public List<InventoryItem> Contents()
        {
            return _contents;
        }

        //Returns item in target inventory if the item was successfully moved
        private void Move(InventoryItem item)
        {
            Debug.Log(item.Name);
            Inventory parent = item.ParentInventory;
            if (ParentInventory != null) Debug.Log(item.Name + " " + item.ParentInventory.Name);
            InventoryItem movedItem = parent == null ? item : parent.RemoveItem(item);
            AddItem(movedItem);
        }

        public void Move(InventoryItem item, int quantity)
        {
            if (!item.IsStackable())
            {
                Move(item);
                return;
            }

            if (quantity > item.Quantity()) quantity = Mathf.FloorToInt(item.Quantity());
            if (quantity <= 0) return;
            item.ParentInventory?.DecrementResource(item.Name, quantity);
            IncrementResource(item.Name, quantity);
        }

        public void MoveAllResources(Inventory target)
        {
            List<InventoryItem> resources = _resources.Values.ToList();
            for (int i = resources.Count - 1; i >= 0; --i)
            {
                target.Move(resources[i], Mathf.FloorToInt(resources[i].Quantity()));
            }
        }

        private void LoadResource(string type, XmlNode root)
        {
            IncrementResource(type, SaveController.ParseIntFromNodeAndString(root, type));
        }

        private void SaveResource(string type, XmlNode root)
        {
            SaveController.CreateNodeAndAppend(type, root, GetResourceQuantity(type));
        }

        public virtual void DestroyItem(InventoryItem item)
        {
            _items.Remove(item);
        }
    }
}