using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating;
using Game.Characters;
using Game.Exploration.Environment;
using Game.Gear;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine.Assertions;

namespace Game.Global
{
    public class Recipe : MyGameObject
    {
        public readonly string Ingredient1;
        public readonly string Ingredient2;
        public readonly int Ingredient1Quantity;
        public readonly int Ingredient2Quantity;
        public const float DurationInHours = 1f;
        private static readonly List<Recipe> _recipes = new List<Recipe>();
        private static bool _loaded;
        public readonly string ProductName;
        public readonly int ProductQuantity;
        private readonly bool _requiresFire;
        public readonly bool IsBuilding;

        private Recipe(string ingredient1, string ingredient2, int ingredient1Quantity, int ingredient2Quantity, string productName, int productQuantity, bool isBuilding, int levelNo) : base(productName,
            GameObjectType.Resource)
        {
            Ingredient1 = ingredient1;
            Ingredient2 = ingredient2;
            Ingredient1Quantity = ingredient1Quantity;
            Ingredient2Quantity = ingredient2Quantity;
            ProductQuantity = productQuantity;
            ProductName = productName;
            _requiresFire = ingredient1 == "Fire" || ingredient2 == "Fire";
            IsBuilding = isBuilding;
            _levelNo = levelNo;
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

        public void Craft()
        {
            switch (ProductName)
            {
                case "Fire":
                    CharacterManager.SelectedCharacter.LightFireAction.Enter();
                    break;
                case "Shelter":
                    Inventory.AddBuilding(new Shelter());
                    break;
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
                case "Leather Plate":
                    Inventory.Move(Armour.Create(ItemQuality.Dark));
                    break;
                case "Smoker":
                    Inventory.AddBuilding(new Smoker());
                    break;
                case "Purifier":
                    Inventory.AddBuilding(new Purifier());
                    break;
                case "Reinforced Leather Plate":
                    Inventory.Move(Armour.Create(ItemQuality.Dull));
                    break;
                case "Metal Plate":
                    Inventory.Move(Armour.Create(ItemQuality.Glowing));
                    break;
                case "Alloy Plate":
                    Inventory.Move(Armour.Create(ItemQuality.Radiant));
                    break;
                case "Living Metal Plate":
                    Inventory.Move(Armour.Create(ItemQuality.Shining));
                    break;
                case "Ice":
                    Inventory.IncrementResource(ProductName, ProductQuantity);
                    break;
                case "Radiance":
                    Inventory.IncrementResource(ProductName, 1);
                    break;
            }
        }

        private bool _unlocked;
        private int _levelNo;

        private bool Available()
        {
            if (_unlocked) return true;
            _unlocked = _levelNo <= EnvironmentManager.CurrentEnvironment.LevelNo;
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
            return _recipes.Any(r => r.CanCraft());
        }

        private static void LoadRecipes()
        {
            if (_loaded) return;
            XmlNode root = Helper.OpenRootNode("Recipes");
            foreach (XmlNode recipeNode in Helper.GetNodesWithName(root, "Recipe"))
            {
                string ingredient1Name = recipeNode.StringFromNode("Ingredient1Name");
                int ingredient1Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
                string ingredient2Name = recipeNode.StringFromNode("Ingredient2Name");
                int ingredient2Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
                string productName = recipeNode.StringFromNode("ProductName");
                int productQuantity = recipeNode.IntFromNode("ProductQuantity");
                bool isBuilding = recipeNode.BoolFromNode("IsBuilding");
                int levelNo = recipeNode.IntFromNode("LevelNo");

                Recipe recipe = new Recipe(ingredient1Name, ingredient2Name, ingredient1Quantity, ingredient2Quantity, productName, productQuantity, isBuilding, levelNo);
                _recipes.Add(recipe);
            }

            _loaded = true;
        }

        public static Recipe FindRecipe(string recipeName)
        {
            return _recipes.FirstOrDefault(r => r.Name == recipeName);
        }

        public int Built()
        {
            return Inventory.GetBuildingCount(ProductName);
        }
    }
}