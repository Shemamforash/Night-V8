﻿using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Loot : ContainerController
    {
        public Loot(Vector2 position) : base(position)
        {
        }

        public void SetResource(ResourceItem item)
        {
            Item = item;
            if (item.Template?.ResourceType == ResourceType.Meat) ImageLocation = "Meat";
        }

        public void SetItem(GearItem item)
        {
            Item = item;
            if (item is Accessory) ImageLocation = "Accessory";
            else if (item is Weapon) ImageLocation = "Weapon";
            else if (item is Inscription) ImageLocation = "Inscription";
        }

        protected override string GetLogText()
        {
            switch (Item)
            {
                case Weapon _:
                    return "Found a weapon";
                case Accessory _:
                    return "Found an accessory";
                case Inscription _:
                    return "Found an inscription";
                case ResourceItem item:
                    return "Found " + item.Name;
            }

            return "";
        }
    }
}