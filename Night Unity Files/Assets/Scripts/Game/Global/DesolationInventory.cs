using System.Collections.Generic;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Global
{
    public class DesolationInventory : Inventory
    {
        public DesolationInventory(string name) : base(name)
        {
            AddResource(InventoryResourceType.Water, 1);
            AddResource(InventoryResourceType.Food, 1);
            AddResource(InventoryResourceType.Fuel, 1);
            AddResource(InventoryResourceType.Scrap, 0.5f);
            AddTestingResources(10);
        }

        public override List<InventoryItem> SortByType()
        {
            List<InventoryItem> sortedItems = new List<InventoryItem>();
            sortedItems.AddRange(InventoryResources());
            sortedItems.AddRange(GetItemsOfType(item => item is Weapon));
            sortedItems.AddRange(GetItemsOfType(item => item is ArmourPlate));
            sortedItems.AddRange(GetItemsOfType(item => item is Accessory));
            return sortedItems;
        }

        public void AddTestingResources(int resourceCount, int noItems = 0)
        {
            IncrementResource(InventoryResourceType.Food, resourceCount);
            IncrementResource(InventoryResourceType.Fuel, resourceCount);
            IncrementResource(InventoryResourceType.Scrap, resourceCount);
            IncrementResource(InventoryResourceType.Water, resourceCount);
            for (int i = 0; i < noItems; ++i)
            {
                AddItem(WeaponGenerator.GenerateWeapon(ItemQuality.Shining));
                AddItem(Accessory.GenerateAccessory(ItemQuality.Shining));
                AddItem(Inscription.GenerateInscription(ItemQuality.Shining));
                AddItem(ArmourPlate.GeneratePlate(ItemQuality.Shining));
            }
        }

        public void Print()
        {
            string contents = "Resources: \n\n";
            foreach (InventoryResource r in InventoryResources())
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
    }
}