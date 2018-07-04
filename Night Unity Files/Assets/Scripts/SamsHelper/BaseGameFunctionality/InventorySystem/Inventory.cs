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
        private readonly bool _isWeightLimited;
        private int _maxWeight;
        private bool _readonly;
        private static bool _loaded;
        private int _currentWeight;
        private static List<AttributeType> _attributeTypes;

        public Inventory(string name, int maxWeight = 0) : base(name, GameObjectType.Inventory)
        {
            LoadResources();
            if (maxWeight == 0) return;
            _maxWeight = maxWeight;
            if (maxWeight != 0) _isWeightLimited = true;
        }

        public void IncreaseMaxWeight(int amount)
        {
            _maxWeight += amount;
        }

        public List<Consumable> Consumables()
        {
            return _consumables.Where(c => c.Quantity() != 0).ToList();
        }

        public static AttributeType StringToAttributeType(string attributeString)
        {
            if (_attributeTypes == null)
            {
                _attributeTypes = new List<AttributeType>();
                foreach (AttributeType attributeType in Enum.GetValues(typeof(AttributeType))) _attributeTypes.Add(attributeType);
            }

            return _attributeTypes.Find(t => t.ToString() == attributeString);
        }

        private static void LoadResources()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Resources");

            foreach (XmlNode resourceNode in root.SelectNodes("Consumable"))
            {
                string name = resourceNode.SelectSingleNode("Name").InnerText;
                string type = resourceNode.SelectSingleNode("Type").InnerText;
                float oasisDR = float.Parse(resourceNode.SelectSingleNode("OasisDropRate").InnerText);
                float steppeDr = float.Parse(resourceNode.SelectSingleNode("SteppeDropRate").InnerText);
                float ruinsDr = float.Parse(resourceNode.SelectSingleNode("RuinsDropRate").InnerText);
                float defilesDr = float.Parse(resourceNode.SelectSingleNode("DefilesDropRate").InnerText);
                float wastelandDr = float.Parse(resourceNode.SelectSingleNode("WastelandDropRate").InnerText);
                string attributeString = resourceNode.SelectSingleNode("Attribute").InnerText;
                ResourceTemplate resourceTemplate = new ResourceTemplate(name, type, oasisDR, steppeDr, ruinsDr, defilesDr, wastelandDr);
                if (attributeString == "") continue;
                AttributeType attribute = StringToAttributeType(attributeString);
                float modifierVal = float.Parse(resourceNode.SelectSingleNode("Modifier").InnerText);
                bool additive = resourceNode.SelectSingleNode("Bonus").InnerText == "+";
                string durationString = resourceNode.SelectSingleNode("Duration").InnerText;
                int duration = 0;
                if (durationString != "")
                {
                    duration = int.Parse(durationString);
                }

                resourceTemplate.SetEffect(attribute, modifierVal, additive, duration);
            }

            foreach (XmlNode resourceNode in root.SelectNodes("Resource"))
            {
                string name = resourceNode.SelectSingleNode("Name").InnerText;
                string type = resourceNode.SelectSingleNode("Type").InnerText;
                new ResourceTemplate(name, type);
            }

            _loaded = true;
        }

        public void AddTestingResources(int resourceCount, int noItems = 0)
        {
            IncrementResource("Fuel", resourceCount);
            IncrementResource("Scrap", resourceCount);
            IncrementResource("Essence", resourceCount);
            for (int i = 0; i < noItems; ++i)
            {
                AddItem(WeaponGenerator.GenerateWeapon(ItemQuality.Shining));
                AddItem(Accessory.GenerateAccessory(ItemQuality.Shining));
                AddItem(Inscription.Generate());
                AddItem(ArmourPlate.Create(ItemQuality.Shining));
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

        public bool InventoryHasSpace(int quantity = 1)
        {
            int newWeight = _currentWeight + quantity;
            return newWeight <= _maxWeight || !_isWeightLimited;
        }

        public bool ContainsItem(InventoryItem item)
        {
            if (!item.IsStackable()) return _items.Contains(item);
            return _resources.ContainsKey(item.Name) && item.Quantity() != 0;
        }

        protected virtual void AddItem(InventoryItem item)
        {
            ++_currentWeight;
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
            if (!_contents.Contains(item)) throw new Exceptions.ItemNotInInventoryException(item.Name);
            _items.Remove(item);
            --_currentWeight;
            if (item is Consumable) _consumables.Remove((Consumable) item);
            _resources.Remove(item.Name);
            UpdateContents();
            return item;
        }

        public void IncrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "increment", amount);
            InventoryItem resource = GetResource(name);
            _currentWeight += amount;
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
            _currentWeight -= amount;
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
            if (!InventoryHasSpace()) return;
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
                int remainingSpace = _maxWeight - _currentWeight;
                quantity = remainingSpace;
            }

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