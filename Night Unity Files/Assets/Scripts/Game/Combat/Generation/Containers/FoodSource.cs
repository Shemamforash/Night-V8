using UnityEngine;

namespace Game.Combat.Generation
{
    public class FoodSource : ContainerController
    {
        public FoodSource(Vector2 position, string name = "") : base(position, "Food")
        {
            string sourceName = "Bush";
            Inventory.IncrementResource("Fruit", 1);
            Inventory.SetReadonly(true);
            PrefabLocation = sourceName;
        }

        public void Change(int polarity)
        {
            int quantity = Inventory.GetResourceQuantity("Fruit"); 
            if (polarity < 0 && quantity > 0) Inventory.DecrementResource("Fruit", 1);
            else if(polarity > 0 && quantity < 1) Inventory.IncrementResource("Fruit", 1);
        }
    }
}