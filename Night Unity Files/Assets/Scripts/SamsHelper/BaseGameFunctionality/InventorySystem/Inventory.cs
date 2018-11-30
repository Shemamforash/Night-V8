using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine.Assertions;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public static class Inventory
    {
        private static readonly Dictionary<string, ResourceItem> _resources = new Dictionary<string, ResourceItem>();
        private static readonly List<Consumable> _consumables = new List<Consumable>();
        private static readonly List<Weapon> _weapons = new List<Weapon>();
        private static readonly List<Armour> _armour = new List<Armour>();
        private static readonly List<Accessory> _accessories = new List<Accessory>();
        public static readonly List<Inscription> Inscriptions = new List<Inscription>();
        private static bool _loaded;
        private static List<AttributeType> _attributeTypes;
        private static readonly List<Building> _buildings = new List<Building>();

        public static Weapon FindWeapon(int id)
        {
            return _weapons.FirstOrDefault(i => i.ID() == id);
        }

        public static Armour FindArmour(int id)
        {
            return _armour.FirstOrDefault(i => i.ID() == id);
        }

        public static Accessory FindAccessory(int id)
        {
            return _accessories.FirstOrDefault(i => i.ID() == id);
        }

        public static void Reset()
        {
            LoadResources();
            _resources.Clear();
            _consumables.Clear();
            _weapons.Clear();
            _armour.Clear();
            _accessories.Clear();
            Inscriptions.Clear();
            _buildings.Clear();
#if UNITY_EDITOR
//            IncrementResource("Salt", 20);
//            IncrementResource("Radiance", 20);
//            IncrementResource("Fuel", 20);
//            IncrementResource("Water", 20);
//            IncrementResource("Essence", 20);
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

        public static void Load(XmlNode root)
        {
            XmlNode inventoryNode = root.GetNode("Inventory");
            XmlNode resourceNode = inventoryNode.SelectSingleNode("Resources");
            foreach (XmlNode node in resourceNode.SelectNodes("Resource")) LoadResource(node);
            XmlNode itemNode = inventoryNode.SelectSingleNode("Items");
            foreach (XmlNode weaponNode in itemNode.SelectSingleNode("Weapons").ChildNodes)
                Move(Weapon.LoadWeapon(weaponNode));
            foreach (XmlNode armourNode in itemNode.SelectSingleNode("ArmourPlates").ChildNodes)
                Move(Armour.LoadArmour(armourNode));
            foreach (XmlNode accessoryNode in itemNode.SelectSingleNode("Accessories").ChildNodes)
                Move(Accessory.LoadAccessory(accessoryNode));
            foreach (XmlNode inscriptionNode in itemNode.SelectSingleNode("Inscriptions").ChildNodes)
                Move(Inscription.LoadInscription(inscriptionNode));
            Building.LoadBuildings(inventoryNode);
        }

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Inventory");
            XmlNode resourceNode = root.CreateChild("Resources");
            InventoryResources().ForEach(r => r.Save(resourceNode));
            XmlNode itemNode = root.CreateChild("Items");
            SaveItems(itemNode, "Weapons", _weapons);
            SaveItems(itemNode, "ArmourPlates", _armour);
            SaveItems(itemNode, "Accessories", _accessories);
            SaveItems(itemNode, "Inscriptions", Inscriptions);
            Building.SaveBuildings(root);
        }

        private static void SaveItems<T>(XmlNode root, string itemType, List<T> items) where T : GearItem
        {
            root = root.CreateChild(itemType);
            items.ForEach(i => i.Save(root));
        }

        private static List<ResourceItem> InventoryResources()
        {
            return _resources.Values.ToList();
        }

        private static ResourceItem AddResource(string name)
        {
            ResourceItem newResourceItem = ResourceTemplate.Create(name);
            if (newResourceItem is Consumable) _consumables.Add((Consumable) newResourceItem);
            _resources.Add(name, newResourceItem);
            return newResourceItem;
        }

        private static ResourceItem GetResource(string resourceName)
        {
            ResourceItem item;
            _resources.TryGetValue(resourceName, out item);
            return item;
        }

        public static void IncrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "increment", amount);
            ResourceItem resourceItem = GetResource(name);
            if (resourceItem == null)
            {
                resourceItem = AddResource(name);
                --amount;
            }

            resourceItem.Increment(amount);
        }

        public static void DecrementResource(string name, int amount)
        {
            if (amount < 0) throw new Exceptions.ResourceValueChangeInvalid(name, "decrement", amount);
            ResourceItem resourceItem = GetResource(name);
            if (resourceItem == null) return;
            if (resourceItem.Quantity() < amount) return;
            resourceItem.Decrement(amount);
            if (resourceItem.Quantity() == 0) _resources.Remove(name);
        }

        public static int GetResourceQuantity(string type)
        {
            return GetResource(type)?.Quantity() ?? 0;
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
                case "Purifier":
                    return _buildings.Count(b => b is Purifier);
                case "Smoker":
                    return _buildings.Count(b => b is Smoker);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static List<Building> Buildings()
        {
            return _buildings;
        }

        public static List<Weapon> GetAvailableWeapons() => GetAvailableItems(_weapons);
        public static List<Accessory> GetAvailableAccessories() => GetAvailableItems(_accessories);
        public static List<Armour> GetAvailableArmour() => GetAvailableItems(_armour);

        private static List<T> GetAvailableItems<T>(List<T> items) where T : GearItem
        {
            return items.FindAll(w => w.EquippedCharacter == null);
        }

        public static void Move(ResourceItem item)
        {
            Assert.IsFalse(item is GearItem);
            IncrementResource(item.Name, item.Quantity());
        }

        private static void SortItem<T>(List<T> items) where T : GearItem
        {
            items.Sort((a, b) =>
            {
                int ret = b.Quality().CompareTo(a.Quality());
                if (ret == 0) ret = b.Name.CompareTo(a.Name);
                return ret;
            });
        }

        public static void Move(Weapon weapon)
        {
            _weapons.Add(weapon);
            _weapons.Sort((a, b) =>
            {
                int ret = b.Quality().CompareTo(a.Quality());
                if (ret == 0) ret = a.WeaponType().CompareTo(b.WeaponType());
                if (ret == 0) ret = b.Name.CompareTo(a.Name);
                return ret;
            });
        }

        public static void Move(Armour armour)
        {
            _armour.Add(armour);
            SortItem(_armour);
        }

        public static void Move(Accessory accessory)
        {
            _accessories.Add(accessory);
            SortItem(_accessories);
        }

        public static void Move(Inscription inscription)
        {
            Inscriptions.Add(inscription);
            SortItem(Inscriptions);
        }

        private static void LoadResource(XmlNode root)
        {
            string type = root.StringFromNode("Template");
            int quantity = root.IntFromNode("Quantity");
            IncrementResource(type, quantity);
        }

        public static void Destroy(Weapon weapon)
        {
            _weapons.Remove(weapon);
        }

        public static void Destroy(Armour armour)
        {
            _armour.Remove(armour);
        }

        public static void Destroy(Accessory accessory)
        {
            _accessories.Remove(accessory);
        }

        public static void Destroy(Inscription inscription)
        {
            Inscriptions.Remove(inscription);
        }
    }
}