﻿using SamsHelper.BaseGameFunctionality.InventorySystem;
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
            if (item.Template?.ResourceType == "Meat") ImageLocation = "Meat";   
        }
        
        public void SetItem(GearItem item)
        {
            Item = item;
        }
    }
}