using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating;
using Facilitating.Persistence;
using Game.Exploration.Environment;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Global
{
    public class Recipe
    {
        private static readonly List<Recipe> _recipes = new List<Recipe>();
        private static bool _loaded;
        private static int _craftingLevel;
        public readonly string Ingredient1, Ingredient2, Name;
        public readonly int Ingredient1Quantity, Ingredient2Quantity, ProductQuantity;
        public readonly RecipeType RecipeType;
        private readonly int _levelNo;

        private Recipe(XmlNode recipeNode)
        {
            Ingredient1 = recipeNode.StringFromNode("Ingredient1Name");
            Ingredient2 = recipeNode.StringFromNode("Ingredient2Name");
            Ingredient1Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
            Ingredient2Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
            ProductQuantity = recipeNode.IntFromNode("ProductQuantity");
            Name = recipeNode.StringFromNode("ProductName");
            string recipeTypeString = recipeNode.StringFromNode("RecipeType");
            switch (recipeTypeString)
            {
                case "BUILDING":
                    RecipeType = RecipeType.Building;
                    break;
                case "FIRE":
                    RecipeType = RecipeType.Fire;
                    break;
                case "RESOURCE":
                    RecipeType = RecipeType.Resource;
                    break;
                case "UPGRADE":
                    RecipeType = RecipeType.Upgrade;
                    break;
                default:
                    Debug.Log(recipeTypeString);
                    break;
            }

            _levelNo = recipeNode.IntFromNode("LevelNo");
        }

        public static void Save(XmlNode doc)
        {
            doc.CreateChild("CraftingLevel", _craftingLevel);
        }

        public static void Load(XmlNode doc)
        {
            _craftingLevel = doc.IntFromNode("CraftingLevel");
        }

        public bool CanCraft()
        {
            float ingredient1OwnedQuantity = Inventory.GetResourceQuantity(Ingredient1);
            if (ingredient1OwnedQuantity < Ingredient1Quantity) return false;
            if (Ingredient2 == "None") return true;
            float ingredient2OwnedQuantity = Inventory.GetResourceQuantity(Ingredient2);
            return ingredient2OwnedQuantity >= Ingredient2Quantity;
        }

        public void ConsumeResources()
        {
            Assert.IsTrue(CanCraft());
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
                case RecipeType.Fire:
                    break;
                case RecipeType.Upgrade:
                    ++_craftingLevel;
                    break;
            }
        }

        private bool Available()
        {
            bool validLevel = _levelNo <= (int) EnvironmentManager.CurrentEnvironmentType();
            if (!validLevel) return false;
            if (RecipeType == RecipeType.Fire) return true;
            if (!Campfire.IsLit()) return false;
            if (RecipeType == RecipeType.Upgrade) return _craftingLevel < _levelNo;
            return _craftingLevel >= _levelNo;
        }

        public static List<Recipe> Recipes()
        {
            LoadRecipes();
            return _recipes.FindAll(r => r.Available());
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
            foreach (XmlNode recipeNode in root.GetNodesWithName("Recipe"))
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