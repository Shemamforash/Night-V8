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
            _inventory.Name = _waterType;
            _inventory.IncrementResource(_waterType, 1);
            PrefabLocation = "Puddle";
            ImageLocation = "Water";
        }

        public void Change(int polarity)
        {
            int quantity = _inventory.GetResourceQuantity(_waterType);
            if (polarity < 0 && quantity > 0) _inventory.DecrementResource(_waterType, 1);
            else if (polarity > 0 && quantity < _capacity) _inventory.IncrementResource(_waterType, 1);
        }
    }
}