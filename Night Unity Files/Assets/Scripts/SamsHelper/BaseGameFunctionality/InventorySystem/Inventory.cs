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
        public readonly List<Weapon> Weapons = new List<Weapon>();
        public readonly List<ArmourPlate> Armour = new List<ArmourPlate>();
        public readonly List<Accessory> Accessories = new List<Accessory>();
        public readonly List<Inscription> Inscriptions = new List<Inscription>();
        private static bool _loaded;
        private static List<AttributeType> _attributeTypes;
        private static readonly List<Inventory> _inventories = new List<Inventory>();

        public static Inventory FindInventory(int inventoryId)
        {
            return _inventories.FirstOrDefault(i => i.ID() == inventoryId);
        }

        public InventoryItem FindItem(int id)
        {
            return _contents.FirstOrDefault(i => i.ID() == id);
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

        public Inventory(string name) : base(name, GameObjectType.Inventory)
        {
            LoadResources();
            _inventories.Add(this);
        }

        public List<Consumable> Consumables()
        {
            return _consumables.Where(c => c.Quantity() != 0).ToList();
        }

        private static void LoadResources()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Resources");

            foreach (XmlNode resourceNode in Helper.GetNodesWithName(root, "Consumable"))
                new ResourceTemplate(resourceNode);

            foreach (XmlNode resourceNode in Helper.GetNodesWithName(root, "Resource"))
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

        public override void Load(XmlNode root)
        {
            XmlNode inventoryNode = root.GetNode("Inventory");
            base.Load(inventoryNode);
            XmlNode resourceNode = root.SelectSingleNode("Resources");
            InventoryResources().ForEach(r => LoadResource(r.Name, resourceNode));
            XmlNode itemNode = root.CreateChild("Items");
            foreach (XmlNode weaponNode in itemNode.SelectNodes("Weapons"))
                AddItem(Weapon.LoadWeapon(weaponNode));
            foreach (XmlNode armourNode in itemNode.SelectNodes("ArmourPlates"))
                AddItem(ArmourPlate.LoadArmour(armourNode));
            foreach (XmlNode accessoryNode in itemNode.SelectNodes("Accessories"))
                AddItem(Accessory.LoadAccessory(accessoryNode));
            foreach (XmlNode inscriptionNode in itemNode.SelectNodes("Inscriptions"))
                AddItem(Inscription.LoadInscription(inscriptionNode));
//            foreach (XmlNode consumableNode in itemNode.SelectNodes("Consumables"))
//                AddResource(Consumable.LoadConsumable(consumableNode));
        }


        public virtual XmlNode Save(XmlNode root)
        {
            root = base.Save(root);
            XmlNode resourceNode = root.CreateChild("Resources");
            InventoryResources().ForEach(r => SaveResource(r.Name, resourceNode));
            XmlNode itemNode = root.CreateChild("Items");
            SaveItems(itemNode, "Weapons", Weapons);
            SaveItems(itemNode, "ArmourPlates", Armour);
            SaveItems(itemNode, "Accessories", Accessories);
            SaveItems(itemNode, "Inscriptions", Inscriptions);
            SaveItems(itemNode, "Consumables", _consumables);
            return root;
        }

        private void SaveItems<T>(XmlNode root, string itemType, List<T> items) where T : MyGameObject
        {
            root = root.CreateChild(itemType);
            items.ForEach(i => i.Save(root));
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
            newResource.SetParentInventory(this);
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

        private void AddItem(InventoryItem item)
        {
            item.SetParentInventory(this);
            _items.Add(item);
            Assert.IsFalse(item is Consumable);
            if (item is Weapon) Weapons.Add((Weapon) item);
            else if (item is ArmourPlate) Armour.Add((ArmourPlate) item);
            else if (item is Accessory) Accessories.Add((Accessory) item);
            else if (item is Inscription) Inscriptions.Add((Inscription) item);
            UpdateContents();
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        private InventoryItem RemoveItem(InventoryItem item)
        {
            if (!_contents.Contains(item)) throw new Exceptions.ItemNotInInventoryException(item.Name);
            _items.Remove(item);
            _consumables.Remove(item as Consumable);
            _resources.Remove(item.Name);
            Weapons.Remove(item as Weapon);
            Armour.Remove(item as ArmourPlate);
            Accessories.Remove(item as Accessory);
            Inscriptions.Remove(item as Inscription);
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
            Inventory parent = item.ParentInventory();
            if (ParentInventory() != null) Debug.Log(item.Name + " " + item.ParentInventory().Name);
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
            item.ParentInventory()?.DecrementResource(item.Name, quantity);
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
            IncrementResource(type, root.IntFromNode(type));
        }

        private void SaveResource(string type, XmlNode root)
        {
            root.CreateChild(type, GetResourceQuantity(type));
        }

        public void DestroyItem(InventoryItem item)
        {
            _items.Remove(item);
            Weapons.Remove(item as Weapon);
            Armour.Remove(item as ArmourPlate);
            Accessories.Remove(item as Accessory);
            Inscriptions.Remove(item as Inscription);
        }
    }
}