using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class WaterSource : ContainerController
    {
        private readonly int _capacity;
        private readonly string _waterType;

        public WaterSource(Vector2 position) : base(position, "Puddle")
        {
            _capacity = Random.Range(0, 3) + 1;
            _waterType = ResourceTemplate.GetWater().Name;
            Inventory.Name = _waterType;
            Inventory.IncrementResource(_waterType, 1);
            Inventory.SetReadonly(true);
            PrefabLocation = "Puddle";
        }

        public void Change(int polarity)
        {
            int quantity = Inventory.GetResourceQuantity(_waterType);
            if (polarity < 0 && quantity > 0) Inventory.DecrementResource(_waterType, 1);
            else if (polarity > 0 && quantity < _capacity) Inventory.IncrementResource(_waterType, 1);
        }
    }
}