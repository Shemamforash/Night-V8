using UnityEngine;

namespace Game.Combat.Generation
{
    public class WaterSource : ContainerController
    {
        private readonly int _capacity;

        public WaterSource(Vector2 position, string name = "") : base(position, "Water")
        {
            _capacity = Random.Range(0, 3) + 1;
            Inventory.IncrementResource("Water", 1);
            Inventory.SetReadonly(true);
            PrefabLocation = "Puddle";
        }

        public void Change(int polarity)
        {
            int quantity = Inventory.GetResourceQuantity("Water");
            if (polarity < 0 && quantity > 0) Inventory.DecrementResource("Water", 1);
            else if (polarity > 0 && quantity < _capacity) Inventory.IncrementResource("Water", 1);
        }
    }
}