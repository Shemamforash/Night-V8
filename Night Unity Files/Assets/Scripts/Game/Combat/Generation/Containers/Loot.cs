using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Loot : ContainerController
    {
        public bool IsValid;

        public Loot(Vector2 position, string name) : base(position, name)
        {
            Inventory.SetReadonly(true);
        }

        public void AddToInventory(InventoryItem item)
        {
            IsValid = true;
            Inventory.Move(item, 1);
        }
    }
}