using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Loot : ContainerController
    {
        public Loot(Vector2 position, InventoryItem item) : base(position)
        {
            SetItem(item);
        }

        public void SetItem(InventoryItem item)
        {
            Item = item;
            if (item.Template.ResourceType == "Meat") ImageLocation = "Meat";   
        }
    }
}