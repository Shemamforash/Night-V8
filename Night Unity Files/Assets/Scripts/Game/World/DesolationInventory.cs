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
            AddResource(InventoryResourceType.PistolMag, 0.5f);
            AddResource(InventoryResourceType.RifleMag, 1f);
            AddResource(InventoryResourceType.ShotgunMag, 1.5f);
            AddResource(InventoryResourceType.SmgMag, 1f);
            AddResource(InventoryResourceType.LmgMag, 2);
        }

        public override List<MyGameObject> SortByType()
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
            IncrementResource(InventoryResourceType.PistolMag, 10);
            IncrementResource(InventoryResourceType.RifleMag, 10);
            IncrementResource(InventoryResourceType.ShotgunMag, 10);
            IncrementResource(InventoryResourceType.SmgMag, 10);
            IncrementResource(InventoryResourceType.LmgMag, 10);
            IncrementResource(InventoryResourceType.Food, 1000);
            IncrementResource(InventoryResourceType.Fuel, 1000);
            IncrementResource(InventoryResourceType.Scrap, 1000);
            IncrementResource(InventoryResourceType.Water, 1000);
            for (int i = 0; i < noItems; ++i)
            {
                AddItem(WeaponGenerator.GenerateWeapon());
                AddItem(GearReader.GenerateArmour());
                AddItem(GearReader.GenerateAccessory());
            }
        }

        public void SetEnemyResources()
        {
            IncrementResource(InventoryResourceType.PistolMag, 1000);
            IncrementResource(InventoryResourceType.RifleMag, 1000);
            IncrementResource(InventoryResourceType.ShotgunMag, 1000);
            IncrementResource(InventoryResourceType.SmgMag, 1000);
            IncrementResource(InventoryResourceType.LmgMag, 1000);
        }
    }
}