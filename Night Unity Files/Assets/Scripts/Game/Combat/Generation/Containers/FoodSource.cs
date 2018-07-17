using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class FoodSource : ContainerController
    {
        private readonly string _foodType;
        
        public FoodSource(Vector2 position) : base(position, "Plant")
        {
            _foodType = ResourceTemplate.GetPlant().Name;
            _inventory.Name = _foodType;
            _inventory.IncrementResource(_foodType, 1);
            ImageLocation = "Plants/" + _foodType;
        }

        public void Change(int polarity)
        {
            int quantity = _inventory.GetResourceQuantity(_foodType); 
            if (polarity < 0 && quantity == 1) _inventory.DecrementResource(_foodType, 1);
            else if(polarity > 0 && quantity == 0) _inventory.IncrementResource(_foodType, 1);
        }
    }
}