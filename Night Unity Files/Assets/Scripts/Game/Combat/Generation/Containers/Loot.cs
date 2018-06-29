using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Loot : ContainerController
    {
        public bool IsValid;

        public Loot(Vector2 position, string name) : base(position, name)
        {
            _inventory.SetReadonly(true);
        }

        public void AddToInventory(InventoryItem item)
        {
            IsValid = true;
            _inventory.Move(item, 1);
        }

        public void IncrementResource(string name, int amount)
        {
            _inventory.IncrementResource(name, amount);
        }
    }
}