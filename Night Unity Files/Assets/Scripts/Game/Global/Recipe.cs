using System.Collections.Generic;
using System.Xml;
using Facilitating;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;

namespace Game.Global
{
    public class Recipe : MyGameObject
    {
        public readonly string Ingredient1;
        public readonly string Ingredient2;
        public readonly int Ingredient1Quantity;
        public readonly int Ingredient2Quantity;
        public InventoryItem Product;
        public readonly float DurationInHours;
        private static readonly List<Recipe> _recipes = new List<Recipe>();
        private static bool _loaded;
        public readonly string ProductName;
        public readonly int ProductQuantity;
        private bool _requiresFire;

        private Recipe(string ingredient1, string ingredient2, int ingredient1Quantity, int ingredient2Quantity, string productName, int productQuantity, float duration) : base(productName, GameObjectType.Resource)
        {
            Ingredient1 = ingredient1;
            Ingredient2 = ingredient2;
            Ingredient1Quantity = ingredient1Quantity;
            Ingredient2Quantity = ingredient2Quantity;
            ProductQuantity = productQuantity;
            ProductName = productName;
            DurationInHours = duration;
        }

        public bool CanCraft()
        {
            if (_requiresFire && !Campfire.IsLit())
            {
                return false;
            }

            float ingredient1OwnedQuantity = WorldState.HomeInventory().GetResourceQuantity(Ingredient1);
            if (ingredient1OwnedQuantity < Ingredient1Quantity) return false;
            if (Ingredient2 == "") return true;
            float ingredient2OwnedQuantity = WorldState.HomeInventory().GetResourceQuantity(Ingredient2);
            return ingredient2OwnedQuantity >= Ingredient2Quantity;
        }

        public bool Craft()
        {
            if (!CanCraft()) return false;
            WorldState.HomeInventory().DecrementResource(Ingredient1, Ingredient1Quantity);
            if (Ingredient2 != "") WorldState.HomeInventory().DecrementResource(Ingredient2, Ingredient2Quantity);
            if (ProductName == "Fire") CharacterManager.SelectedCharacter.LightFireAction.Enter();
            return true;
        }

        private bool _unlocked;

        private bool Available()
        {
            if (_unlocked) return true;
            _unlocked = CanCraft();
            return _unlocked;
        }

        public static List<Recipe> Recipes()
        {
            LoadRecipes();
            List<Recipe> availableRecipes = new List<Recipe>();
            _recipes.ForEach(r =>
            {
                if (!r.Available()) return;
                availableRecipes.Add(r);
            });
            return availableRecipes;
        }

        private static void LoadRecipes()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Recipes");
            foreach (XmlNode recipeNode in Helper.GetNodesWithName(root, "Recipe"))
            {
                string ingredient1Name = Helper.GetNodeText(recipeNode, "Ingredient1Name");
                int ingredient1Quantity = Helper.IntFromNode(recipeNode, "Ingredient1Quantity");
                string ingredient2Name = Helper.GetNodeText(recipeNode, "Ingredient2Name");
                int ingredient2Quantity = Helper.IntFromNode(recipeNode, "Ingredient1Quantity");
                string productName = Helper.GetNodeText(recipeNode, "ProductName");
                int productQuantity = Helper.IntFromNode(recipeNode, "ProductQuantity");

                Recipe recipe = new Recipe(ingredient1Name, ingredient2Name, ingredient1Quantity, ingredient2Quantity, productName, productQuantity, 1);
                _recipes.Add(recipe);
            }

            _loaded = true;
        }
    }
}