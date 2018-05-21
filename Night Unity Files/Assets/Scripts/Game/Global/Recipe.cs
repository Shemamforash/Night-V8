using System;
using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace Game.Global
{
    public class Recipe
    {
        public readonly InventoryResourceType Ingredient1;
        public readonly InventoryResourceType Ingredient2;
        public readonly int Ingredient1Quantity;
        public readonly int Ingredient2Quantity;
        public InventoryItem Product;
        public readonly float DurationInHours;
        private static List<Recipe> _recipes;

        public Recipe(string ingredient1, string ingredient2, int ingredient1Quantity, int ingredient2Quantity, float duration)
        {
            Ingredient1 = IngredientNameToType(ingredient1);
            Ingredient2 = ingredient2 == "" ? InventoryResourceType.None : IngredientNameToType(ingredient2);
            Ingredient1Quantity = ingredient1Quantity;
            Ingredient2Quantity = ingredient2Quantity;
            DurationInHours = duration;
            _recipes.Add(this);
        }

        public bool CanCraft()
        {
            float ingredient1OwnedQuantity = WorldState.HomeInventory().GetResourceQuantity(Ingredient1);
            if (ingredient1OwnedQuantity < Ingredient1Quantity) return false;
            if (Ingredient2 == InventoryResourceType.None) return true;
            float ingredient2OwnedQuantity = WorldState.HomeInventory().GetResourceQuantity(Ingredient2);
            return ingredient2OwnedQuantity >= Ingredient2Quantity;
        }

        public bool Craft()
        {
            if (!CanCraft()) return false;
            WorldState.HomeInventory().DecrementResource(Ingredient1, Ingredient1Quantity);
            if (Ingredient2 != InventoryResourceType.None) WorldState.HomeInventory().DecrementResource(Ingredient2, Ingredient2Quantity);
            return true;
        }

        private static InventoryResourceType IngredientNameToType(string ingredientName)
        {
            foreach (InventoryResourceType inventoryResourceType in Enum.GetValues(typeof(InventoryResourceType)))
            {
                if (inventoryResourceType.ToString() == ingredientName)
                {
                    return inventoryResourceType;
                }
            }

            throw new ArgumentOutOfRangeException("Unknown ingredient type: '" + ingredientName + "'");
        }

        public static List<Recipe> Recipes()
        {
            return _recipes;
        }
    }
}