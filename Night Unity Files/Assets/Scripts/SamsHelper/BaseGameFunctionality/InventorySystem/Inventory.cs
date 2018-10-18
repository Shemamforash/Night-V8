using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public static class Inventory
    {
        private static readonly Dictionary<string, InventoryItem> _resources = new Dictionary<string, InventoryItem>();
        private static readonly List<InventoryItem> _items = new List<InventoryItem>();
        private static readonly List<InventoryItem> _contents = new List<InventoryItem>();
        private static readonly List<Consumable> _consumables = new List<Consumable>();
        private static readonly List<Weapon> _weapons = new List<Weapon>();
        private static readonly List<Armour> _armour = new List<Armour>();
        private static readonly List<Accessory> _accessories = new List<Accessory>();
        public static readonly List<Inscription> Inscriptions = new List<Inscription>();
        private static bool _loaded;
        private static List<AttributeType> _attributeTypes;
        private static readonly List<Building> _buildings = new List<Building>();

        public static InventoryItem FindItem(int id)
        {
            return _contents.FirstOrDefault(i => i.ID() == id);
        }

        public static void Reset()
        {
            LoadResources();
            _resources.Clear();
            _items.Clear();
            _contents.Clear();
            _consumables.Clear();
            _weapons.Clear();
            _armour.Clear();
            _accessories.Clear();
            Inscriptions.Clear();
            _buildings.Clear();
#if UNITY_EDITOR
            IncrementResource("Salt", 20);
            IncrementResource("Radiance", 20);
            IncrementResource("Fuel", 20);
            IncrementResource("Water", 20);
            IncrementResource("Essence", 20);
#endif
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

        public static List<Consumable> Consumables()
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

        public static void Print()
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

        public static void Load(XmlNode root)
        {
            XmlNode inventoryNode = root.GetNode("Inventory");
            XmlNode resourceNode = inventoryNode.SelectSingleNode("Resources");
            InventoryResources().ForEach(r => LoadResource(r.Name, resourceNode));
            XmlNode itemNode = inventoryNode.SelectSingleNode("Items");
            foreach (XmlNode weaponNode in itemNode.SelectSingleNode("Weapons").ChildNodes)
                AddItem(Weapon.LoadWeapon(weaponNode));
            foreach (XmlNode armourNode in itemNode.SelectSingleNode("ArmourPlates").ChildNodes)
                AddItem(Armour.LoadArmour(armourNode));
            foreach (XmlNode accessoryNode in itemNode.SelectSingleNode("Accessories").ChildNodes)
                AddItem(Accessory.LoadAccessory(accessoryNode));
            foreach (XmlNode inscriptionNode in itemNode.SelectSingleNode("Inscriptions").ChildNodes)
                AddItem(Inscription.LoadInscription(inscriptionNode));
        }

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Inventory");
            XmlNode resourceNode = root.CreateChild("Resources");
            InventoryResources().ForEach(r => SaveResource(r.Name, resourceNode));
            XmlNode itemNode = root.CreateChild("Items");
            SaveItems(itemNode, "Weapons", _weapons);
            SaveItems(itemNode, "ArmourPlates", _armour);
            SaveItems(itemNode, "Accessories", _accessories);
            SaveItems(itemNode, "Inscriptions", Inscriptions);
            _buildings.ForEach(b => b.Save(root));
        }

        private static void SaveItems<T>(XmlNode root, string itemType, List<T> items) where T : MyGameObject
        {
            root = root.CreateChild(itemType);
            items.ForEach(i => i.Save(root));
        }

        private static List<InventoryItem> InventoryResources()
        {
            return _resources.Values.ToList();
        }

        private static List<InventoryItem> Items()
        {
            return _items;
        }

        private static InventoryItem AddResource(string name)
        {
            InventoryItem newResource = ResourceTemplate.Create(name);
            if (newResource is Consumable)
            {
                _consumables.Add((Consumable) newResource);
            }

            _resources.Add(name, newResource);
            return newResource;
        }

        private static InventoryItem GetResource(string resourceName)
        {
            InventoryItem item;
            _resources.TryGetValue(resourceName, out item);
            return item;
        }

        public static bool ContainsItem(InventoryItem item)
        {
            if (!item.IsStackable()) return _items.Contains(item);
            return _resources.ContainsKey(item.Name) && item.Quantity() != 0;
        }

        private static void AddItem(InventoryItem item)
        {
            _items.Add(item);
            Assert.IsFalse(item is Consumable);
            if (item is Weapon) _weapons.Add((Weapon) item);
            else if (item is Armour) _armour.Add((Armour) item);
            else if (item is Accessory) _accessories.Add((Accessory) item);
            else if (item is Inscription) Inscriptions.Add((Inscription) item);
            UpdateContents();
        }

        //Returns item if the item was successfully removed
        //Returns null if the item could not be removed (stackable but 0)
        //Throws an error if the item was not in the inventory
        private static InventoryItem RemoveItem(InventoryItem item)
        {
            if (!_contents.Contains(item)) throw new Exceptions.ItemNotInInventoryException(item.Name);
            _items.Remove(item);
            _consumables.Remove(item as Consumable);
            _resources.Remove(item.Name);
            _weapons.Remove(item as Weapon);
            _armour.Remove(item as Armour);
            _accessories.Remove(item as Accessory);
            Inscriptions.Remove(item as Inscription);
            UpdateContents();
            return item;
        }

        public static void IncrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "increment", amount);
            InventoryItem resource = GetResource(name) ?? AddResource(name);
            resource.Increment(amount);
            UpdateContents();
        }

        public static void DecrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "decrement", amount);
            InventoryItem resource = GetResource(name);
            if (resource == null) return;
            if (resource.Quantity() < amount) return;
            resource.Decrement(amount);
            if (resource.Quantity() == 0) RemoveItem(resource);
            UpdateContents();
        }

        public static int GetResourceQuantity(string type)
        {
            return GetResource(type)?.Quantity() ?? 0;
        }

        private static void UpdateContents()
        {
            _contents.Clear();
            _items.ForEach(i => _contents.Add(i));
            foreach (InventoryItem r in _resources.Values)
            {
                Assert.IsTrue(r.Quantity() > 0);
                _contents.Add(r);
            }
        }

        public static List<InventoryItem> Contents()
        {
            return _contents;
        }

        public static void UpdateBuildings()
        {
            _buildings.ForEach(b => b.Update());
        }

        public static void AddBuilding(Building building)
        {
            _buildings.Add(building);
        }

        public static int GetBuildingCount(string buildingName)
        {
            switch (buildingName)
            {
                case "Shelter":
                    return _buildings.Count(b => b is Shelter);
                case "Trap":
                    return _buildings.Count(b => b is Trap);
                case "Water Collector":
                    return _buildings.Count(b => b is WaterCollector);
                case "Condenser":
                    return _buildings.Count(b => b is Condenser);
                case "Essence Filter":
                    return _buildings.Count(b => b is EssenceFilter);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static List<Building> Buildings()
        {
            return _buildings;
        }

        //Returns item in target inventory if the item was successfully moved
        private static void Move(InventoryItem item)
        {
            AddItem(item);
        }

        public static List<Weapon> GetAvailableWeapons() => GetAvailableItems(_weapons);
        public static List<Accessory> GetAvailableAccessories() => GetAvailableItems(_accessories);
        public static List<Armour> GetAvailableArmour() => GetAvailableItems(_armour);

        private static List<T> GetAvailableItems<T>(List<T> items) where T : GearItem
        {
            return items.FindAll(w => w.EquippedCharacter == null || w.EquippedCharacter == CharacterManager.SelectedCharacter);
        }

        public static void Move(InventoryItem item, int quantity)
        {
            if (!item.IsStackable())
            {
                Move(item);
                return;
            }

            if (quantity > item.Quantity()) quantity = Mathf.FloorToInt(item.Quantity());
            if (quantity <= 0) return;
            IncrementResource(item.Name, quantity);
        }

        private static void LoadResource(string type, XmlNode root)
        {
            IncrementResource(type, root.IntFromNode(type.Replace(" ", "_")));
        }

        private static void SaveResource(string type, XmlNode root)
        {
            root.CreateChild(type.Replace(" ", "_"), GetResourceQuantity(type));
        }

        public static void DestroyItem(InventoryItem item)
        {
            _items.Remove(item);
            _weapons.Remove(item as Weapon);
            _armour.Remove(item as Armour);
            _accessories.Remove(item as Accessory);
            Inscriptions.Remove(item as Inscription);
        }
    }
}