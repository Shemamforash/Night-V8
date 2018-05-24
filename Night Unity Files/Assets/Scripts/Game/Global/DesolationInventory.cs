﻿using System;
using System.Collections.Generic;
using System.Xml;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Global
{
    public class DesolationInventory : Inventory
    {
        private static bool _loaded;
        private static Dictionary<InventoryResourceType, float> _resources = new Dictionary<InventoryResourceType, float>();

        public DesolationInventory(string name) : base(name)
        {
            LoadResources();
            foreach (InventoryResourceType resourceType in _resources.Keys)
            {
                AddResource(resourceType, _resources[resourceType]);
            }
            AddTestingResources(10);
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

                foreach (InventoryResourceType resourceType in Enum.GetValues(typeof(InventoryResourceType)))
                {
                    if (resourceType.ToString() == name)
                    {
                        _resources.Add(resourceType, weight);
                    }
                }
            }

            _loaded = true;
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
                AddItem(ArmourPlate.Create("Living Metal Plate"));
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