using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class FoodSource : ContainerController
    {
        private readonly string _foodType;
        
        public FoodSource(Vector2 position) : base(position, "")
        {
            _foodType = ResourceTemplate.GetPlant().Name;
            Inventory.Name = _foodType;
            Inventory.IncrementResource(_foodType, 1);
            Inventory.SetReadonly(true);
        }

        public void Change(int polarity)
        {
            int quantity = Inventory.GetResourceQuantity(_foodType); 
            if (polarity < 0 && quantity == 1) Inventory.DecrementResource(_foodType, 1);
            else if(polarity > 0 && quantity == 0) Inventory.IncrementResource(_foodType, 1);
        }
    }
}