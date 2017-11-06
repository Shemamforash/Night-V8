using System.Collections.Generic;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.World
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory(string name) : base(name)
        {
            AddResource(InventoryResourceType.Water, 1);
            AddResource(InventoryResourceType.Food, 1);
            AddResource(InventoryResourceType.Fuel, 1);
            AddResource(InventoryResourceType.Scrap, 0.5f);
            AddResource(InventoryResourceType.Ammo, 0.1f);
        }

        public List<MyGameObject> SortByType()
        {
            List<MyGameObject> sortedItems = new List<MyGameObject>();
            sortedItems.AddRange(Resources());
            sortedItems.AddRange(GetItemsOfType(item => item is Weapon));
            sortedItems.AddRange(GetItemsOfType(item => item is Armour));
            sortedItems.AddRange(GetItemsOfType(item => item is Accessory));
            return sortedItems;
        }
        
        public void AddTestingResources(int noItems = 0)
        {
            IncrementResource(InventoryResourceType.Ammo, 1000);
            IncrementResource(InventoryResourceType.Food, 100);
            IncrementResource(InventoryResourceType.Fuel, 100);
            IncrementResource(InventoryResourceType.Scrap, 100);
            IncrementResource(InventoryResourceType.Water, 100);
            for (int i = 0; i < noItems; ++i)
            {
                AddItem(WeaponGenerator.GenerateWeapon());
                AddItem(GearReader.GenerateArmour());
                AddItem(GearReader.GenerateAccessory());
            }
        }
        
    }
}