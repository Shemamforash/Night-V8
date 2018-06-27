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
using Random = UnityEngine.Random;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Inventory : MyGameObject, IPersistenceTemplate
    {
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private readonly List<InventoryItem> _resources = new List<InventoryItem>();
        private bool _isWeightLimited;
        private int _maxWeight;
        private readonly List<InventoryItem> _contents = new List<InventoryItem>();
        private bool _readonly;
        private static bool _loaded;
        private static readonly List<ResourceTemplate> resourceTemplates = new List<ResourceTemplate>();
        private readonly List<Consumable> _consumables = new List<Consumable>();
        public int Weight;

        public Inventory(string name, int maxWeight = 0) : base(name, GameObjectType.Inventory)
        {
            LoadResources();
            foreach (ResourceTemplate template in resourceTemplates) AddResource(template);
//            AddTestingResources(10);
            if (maxWeight == 0) return;
            MaxWeight = maxWeight;
            _isWeightLimited = true;
        }

        public List<Consumable> Consumables()
        {
            return _consumables.Where(c => c.Quantity() != 0).ToList();
        }

        private void AddResource(ResourceTemplate template)
        {
            if (_resources.FirstOrDefault(r => r.Name == template.Name) != null) throw new Exceptions.ResourceAlreadyExistsException(template.Name);
            InventoryItem newResource = template.Create();
            newResource.ParentInventory = this;
            if (newResource is Consumable)
            {
                _consumables.Add((Consumable) newResource);
                newResource.Increment(Random.Range(1, 5));
            }

            _resources.Add(newResource);
        }

        private static void LoadResources()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Resources");
            foreach (XmlNode resourceNode in root.SelectNodes("Resource"))
            {
                ResourceTemplate resourceTemplate;
                string name = resourceNode.SelectSingleNode("Name").InnerText;
                string environment = resourceNode.SelectSingleNode("Environment").InnerText;
                string region = resourceNode.SelectSingleNode("Region").InnerText;
                string droplocation = resourceNode.SelectSingleNode("Drop").InnerText;
                float weight = float.Parse(resourceNode.SelectSingleNode("Weight").InnerText);
                bool consumable = resourceNode.SelectSingleNode("Consumable").InnerText == "TRUE";
                if (consumable)
                {
                    string effect1 = resourceNode.SelectSingleNode("Effect1").InnerText;
                    string effect2 = resourceNode.SelectSingleNode("Effect2").InnerText;
                    float duration1 = float.Parse(resourceNode.SelectSingleNode("Duration1").InnerText);
                    float duration2 = float.Parse(resourceNode.SelectSingleNode("Duration2").InnerText);
                    resourceTemplate = new ResourceTemplate(name, weight, environment, region, droplocation, effect1, effect2, duration1, duration2);
                }
                else
                {
                    resourceTemplate = new ResourceTemplate(name, weight, environment, region, droplocation);
                }

                resourceTemplates.Add(resourceTemplate);
            }

            _loaded = true;
        }

        public void AddTestingResources(int resourceCount, int noItems = 0)
        {
            IncrementResource("Fruit", resourceCount);
            IncrementResource("Fuel", resourceCount);
            IncrementResource("Scrap", resourceCount);
            IncrementResource("Water", resourceCount);
            IncrementResource("Essence", resourceCount);
            for (int i = 0; i < noItems; ++i)
            {
                AddItem(WeaponGenerator.GenerateWeapon(ItemQuality.Shining));
                AddItem(Accessory.GenerateAccessory(ItemQuality.Shining));
                AddItem(Inscription.Generate());
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

        public int MaxWeight
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

        private List<InventoryItem> InventoryResources()
        {
            return _resources;
        }

        protected List<InventoryItem> Items()
        {
            return _items;
        }

        public List<InventoryItem> GetItemsOfType(Func<InventoryItem, bool> typeCheck)
        {
            return _items.Where(typeCheck).ToList();
        }

        public InventoryItem GetResource(string resourceName)
        {
            InventoryItem found = _resources.FirstOrDefault(item => item.Name == resourceName);
            if (found == null) throw new Exceptions.ResourceDoesNotExistException(resourceName);
            return found;
        }

        private bool InventoryHasSpace(int weight)
        {
            int newWeight = Weight + weight;
            return newWeight <= MaxWeight || !_isWeightLimited;
        }

        public bool ContainsItem(InventoryItem item)
        {
            if (!item.IsStackable()) return _items.Contains(item);
            return _resources.Contains(item) && item.Quantity() != 0;
        }

        protected virtual void AddItem(InventoryItem item)
        {
            ++Weight;
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
            --Weight;
            UpdateContents();
            if (item is Consumable)
            {
                _consumables.Remove((Consumable) item);
            }

            return item;
        }

        public void IncrementResource(string type, int amount)
        {
            InventoryItem resource = GetResource(type);
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "increment", amount);
            Weight += amount;
            resource.Increment(amount);
            UpdateContents();
        }

        public void DecrementResource(string type, int amount)
        {
            InventoryItem resource = GetResource(type);
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(resource.Name, "decrement", amount);
            if (resource.Quantity() < amount) return;
            Weight -= amount;
            resource.Decrement(amount);
            UpdateContents();
        }

        public int GetResourceQuantity(string type)
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
            if (!InventoryHasSpace(1)) return;
            Inventory parent = item.ParentInventory;
            if (ParentInventory != null) Debug.Log(item.Name + " " + item.ParentInventory.Name);
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
            if (!InventoryHasSpace(quantity))
            {
                int remainingSpace = MaxWeight - Weight;
                quantity = remainingSpace;
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

        public void DestroyItem(InventoryItem item)
        {
            _items.Remove(item);
        }
    }
}