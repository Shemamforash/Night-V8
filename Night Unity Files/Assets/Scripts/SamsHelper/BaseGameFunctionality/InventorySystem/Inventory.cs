using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory : MyGameObject, IPersistenceTemplate
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly List<InventoryItem> _resources = new List<InventoryItem>();
        private bool _isWeightLimited;
        private float _maxWeight;
        private readonly List<InventoryItem> _contents = new List<InventoryItem>();
        private bool _readonly;
        private static bool _loaded;
        private static readonly Dictionary<string, float> resourceNames = new Dictionary<string, float>();
        private readonly List<Consumable> _consumables = new List<Consumable>();

        public Inventory(string name, float maxWeight = 0) : base(name, GameObjectType.Inventory)
        {
            LoadResources();
            foreach (string resourceType in resourceNames.Keys)
            {
                AddResource(resourceType, resourceNames[resourceType]);
            }

//            AddTestingResources(10);
            if (maxWeight == 0) return;
            MaxWeight = maxWeight;
            _isWeightLimited = true;
        }

        public List<Consumable> Consumables()
        {
            return _consumables;
        }

        private static void LoadResources()
        {
            if (_loaded) return;
            TextAsset resourceFile = Resources.Load<TextAsset>("XML/Resources");
            XmlDocument resourceXml = new XmlDocument();
            resourceXml.LoadXml(resourceFile.text);
            XmlNode root = resourceXml.SelectSingleNode("Resources");
            foreach (XmlNode resourceNode in root.SelectNodes("Resource"))
            {
                string name = resourceNode.SelectSingleNode("Name").InnerText;
                float weight = float.Parse(resourceNode.SelectSingleNode("Weight").InnerText);
                resourceNames.Add(name, weight);
            }

            _loaded = true;
        }

        public void AddTestingResources(int resourceCount, int noItems = 0)
        {
            IncrementResource("Food", resourceCount);
            IncrementResource("Fuel", resourceCount);
            IncrementResource("Scrap", resourceCount);
            IncrementResource("Water", resourceCount);
            for (int i = 0; i < noItems; ++i)
            {
                AddItem(WeaponGenerator.GenerateWeapon(ItemQuality.Shining));
                AddItem(Accessory.GenerateAccessory(ItemQuality.Shining));
                AddItem(Inscription.GenerateInscription(ItemQuality.Shining));
                AddItem(ArmourPlate.Create("Living Metal Plate"));
            }
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

        public void SetReadonly(bool readOnly)
        {
            _readonly = readOnly;
        }
        
        public bool IsBottomless()
        {
            return !_isWeightLimited;
        }

        protected List<InventoryItem> InventoryResources()
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

        public InventoryItem GetResource(string resourceName)
        {
            InventoryItem found = _resources.FirstOrDefault(item => item.Name == resourceName);
            if (found == null) throw new Exceptions.ResourceDoesNotExistException(resourceName);
            return found;
        }

        private bool InventoryHasSpace(float weight)
        {
            return !(Weight + weight > MaxWeight + 0.0001f) || !_isWeightLimited;
        }

        public bool ContainsItem(InventoryItem item)
        {
            if (!item.IsStackable()) return _items.Contains(item);
            return _resources.Contains(item) && item.Quantity() != 0;
        }

        protected virtual void AddItem(InventoryItem item)
        {
            Weight += item.Weight;
            item.ParentInventory = this;
            _items.Add(item);
            if (item is Consumable)
            {
                _consumables.Add((Consumable) item);
            }
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
            if (item is Consumable)
            {
                _consumables.Remove((Consumable) item);
            }
            return item;
        }

        protected void AddResource(string type, float weight)
        {
            if (_resources.FirstOrDefault(r => r.Name == type) != null) throw new Exceptions.ResourceAlreadyExistsException(type);
            InventoryItem newResource = new InventoryItem(type, GameObjectType.Resource, weight);
            newResource.SetStackable(true);
            newResource.ParentInventory = this;
            if (newResource is Consumable)
            {
                _consumables.Add((Consumable) newResource);
            }
            _resources.Add(newResource);
        }

        public void IncrementResource(string type, int amount)
        {
            InventoryItem resource = GetResource(type);
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "increment", amount);
            Weight += resource.Weight * amount;
            resource.Increment(amount);
            UpdateContents();
        }

        public void DecrementResource(string type, int amount)
        {
            InventoryItem resource = GetResource(type);
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "decrement", amount);
            if (resource.Quantity() < amount) return;
            Weight -= resource.Weight * amount;
            resource.Decrement(amount);
            UpdateContents();
        }

        public float GetResourceQuantity(string type)
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
        private void Move(InventoryItem item)
        {
            if (InventoryHasSpace(item.Weight)) return;
            Inventory parent = item.ParentInventory;
            InventoryItem movedItem = parent == null ? item : parent.RemoveItem(item);
            AddItem(movedItem);
        }

        public void Move(InventoryItem item, int quantity)
        {
            if (_readonly) return;

            if (!item.IsStackable())
            {
                Move(item);
                return;
            }

            if (quantity > item.Quantity()) quantity = Mathf.FloorToInt(item.Quantity());
            if (!InventoryHasSpace(item.Weight * quantity))
            {
                float remainingSpace = MaxWeight - Weight;
                quantity = (int) Math.Floor(remainingSpace / item.Weight);
            }

            if (quantity <= 0) return;
            item.ParentInventory?.DecrementResource(item.Name, quantity);
            IncrementResource(item.Name, quantity);
        }

        public void MoveAllResources(Inventory target)
        {
            foreach (InventoryItem resource in _resources) target.Move(resource, Mathf.FloorToInt(resource.Quantity()));
        }

        private void LoadResource(string type, XmlNode root)
        {
            IncrementResource(type, SaveController.ParseIntFromNodeAndString(root, type));
        }

        private void SaveResource(string type, XmlNode root)
        {
            SaveController.CreateNodeAndAppend(type, root, GetResourceQuantity(type));
        }

        public List<InventoryItem> SortByType()
        {
            List<InventoryItem> sortedItems = new List<InventoryItem>();
            sortedItems.AddRange(InventoryResources());
            sortedItems.AddRange(GetItemsOfType(item => item is Weapon));
            sortedItems.AddRange(GetItemsOfType(item => item is ArmourPlate));
            sortedItems.AddRange(GetItemsOfType(item => item is Accessory));
            return sortedItems;
        }
    }
}