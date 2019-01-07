using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating;
using Game.Exploration.Environment;
using Game.Gear;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Global
{
    public class Recipe
    {
        public readonly string Ingredient1;
        public readonly string Ingredient2;
        public readonly int Ingredient1Quantity;
        public readonly int Ingredient2Quantity;
        private static readonly List<Recipe> _recipes = new List<Recipe>();
        private static bool _loaded;
        public readonly string Name;
        public readonly int ProductQuantity;
        private readonly bool _requiresFire;
        public readonly RecipeType RecipeType;
        private bool _unlocked;
        private readonly int _levelNo;

        private Recipe(XmlNode recipeNode)
        {
            Ingredient1 = recipeNode.StringFromNode("Ingredient1Name");
            Ingredient2 = recipeNode.StringFromNode("Ingredient2Name");
            Ingredient1Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
            Ingredient2Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
            ProductQuantity = recipeNode.IntFromNode("ProductQuantity");
            Name = recipeNode.StringFromNode("ProductName");
            _requiresFire = Ingredient1 == "Fire" || Ingredient2 == "Fire";
            string recipeTypeString = recipeNode.StringFromNode("RecipeType");
            switch (recipeTypeString)
            {
                case "BUILDING":
                    RecipeType = RecipeType.Building;
                    break;
                case "OTHER":
                    RecipeType = RecipeType.Other;
                    break;
                case "RESOURCE":
                    RecipeType = RecipeType.Resource;
                    break;
                default:
                    Debug.Log(recipeTypeString);
                    break;
            }

            _levelNo = recipeNode.IntFromNode("LevelNo");
        }

        public bool CanCraft()
        {
            if (_requiresFire && !Campfire.IsLit()) return false;
            float ingredient1OwnedQuantity = Inventory.GetResourceQuantity(Ingredient1);
            if (ingredient1OwnedQuantity < Ingredient1Quantity) return false;
            if (Ingredient2 == "None") return true;
            float ingredient2OwnedQuantity = Inventory.GetResourceQuantity(Ingredient2);
            return ingredient2OwnedQuantity >= Ingredient2Quantity;
        }

        public void ConsumeResources()
        {
            Assert.IsTrue(CanCraft());
            if (!CanCraft()) return;
            Inventory.DecrementResource(Ingredient1, Ingredient1Quantity);
            if (Ingredient2 != "None") Inventory.DecrementResource(Ingredient2, Ingredient2Quantity);
        }

        private void Build()
        {
            switch (Name)
            {
                case "Trap":
                    Inventory.AddBuilding(new Trap());
                    break;
                case "Water Collector":
                    Inventory.AddBuilding(new WaterCollector());
                    break;
                case "Condenser":
                    Inventory.AddBuilding(new Condenser());
                    break;
                case "Essence Filter":
                    Inventory.AddBuilding(new EssenceFilter());
                    break;
                case "Smoker":
                    Inventory.AddBuilding(new Smoker());
                    break;
                case "Purifier":
                    Inventory.AddBuilding(new Purifier());
                    break;
            }
        }

        private void CraftOther()
        {
            Assert.IsTrue(Name == "Fire");
        }

        public void Craft()
        {
            switch (RecipeType)
            {
                case RecipeType.Building:
                    Build();
                    break;
                case RecipeType.Resource:
                    Inventory.IncrementResource(Name, ProductQuantity);
                    break;
                case RecipeType.Other:
                    CraftOther();
                    break;
            }
        }

        private bool Available()
        {
            if (_unlocked) return true;
            _unlocked = _levelNo <= (int) EnvironmentManager.CurrentEnvironmentType();
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

        public static bool RecipesAvailable()
        {
            LoadRecipes();
            bool recipesAvailable = _recipes.Any(r => r.CanCraft());
            return recipesAvailable;
        }

        private static void LoadRecipes()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Recipes");
            foreach (XmlNode recipeNode in Helper.GetNodesWithName(root, "Recipe"))
            {
                Recipe recipe = new Recipe(recipeNode);
                _recipes.Add(recipe);
            }

            _loaded = true;
        }

        public static Recipe FindRecipe(string recipeName)
        {
            LoadRecipes();
            return _recipes.FirstOrDefault(r => r.Name == recipeName);
        }

        public int Built()
        {
            return Inventory.GetBuildingCount(Name);
        }
    }
}