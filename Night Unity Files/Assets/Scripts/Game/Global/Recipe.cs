using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating;
using Game.Characters;
using Game.Gear;
using Game.Gear.Armour;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Global
{
    public class Recipe : MyGameObject
    {
        public readonly string Ingredient1;
        public readonly string Ingredient2;
        public readonly int Ingredient1Quantity;
        public readonly int Ingredient2Quantity;
        public readonly float DurationInHours;
        private static readonly List<Recipe> _recipes = new List<Recipe>();
        private static bool _loaded;
        public readonly string ProductName;
        public readonly int ProductQuantity;
        private readonly bool _requiresFire;

        private Recipe(string ingredient1, string ingredient2, int ingredient1Quantity, int ingredient2Quantity, string productName, int productQuantity, float duration) : base(productName,
            GameObjectType.Resource)
        {
            Ingredient1 = ingredient1;
            Ingredient2 = ingredient2;
            Ingredient1Quantity = ingredient1Quantity;
            Ingredient2Quantity = ingredient2Quantity;
            ProductQuantity = productQuantity;
            ProductName = productName;
            DurationInHours = duration;
            _requiresFire = ingredient1 == "Fire" || ingredient2 == "Fire";
        }

        private bool CanCraft()
        {
            if (_requiresFire && !Campfire.IsLit()) return false;
            float ingredient1OwnedQuantity = WorldState.HomeInventory().GetResourceQuantity(Ingredient1);
            if (ingredient1OwnedQuantity < Ingredient1Quantity) return false;
            if (Ingredient2 == "None") return true;
            float ingredient2OwnedQuantity = WorldState.HomeInventory().GetResourceQuantity(Ingredient2);
            return ingredient2OwnedQuantity >= Ingredient2Quantity;
        }

        public bool Craft()
        {
            if (!CanCraft()) return false;
            WorldState.HomeInventory().DecrementResource(Ingredient1, Ingredient1Quantity);
            if (Ingredient2 != "None") WorldState.HomeInventory().DecrementResource(Ingredient2, Ingredient2Quantity);
            CreateProduct();
            return true;
        }

        private void CreateProduct()
        {
            switch (ProductName)
            {
                case "Fire":
                    CharacterManager.SelectedCharacter.LightFireAction.Enter();
                    break;
                case "Shelter":
                    WorldState.HomeInventory().AddBuilding(new Shelter());
                    break;
                case "Trap":
                    WorldState.HomeInventory().AddBuilding(new Trap());
                    break;
                case "Water Collector":
                    WorldState.HomeInventory().AddBuilding(new WaterCollector());
                    break;
                case "Condenser":
                    WorldState.HomeInventory().AddBuilding(new Condenser());
                    break;
                case "Essence Filter":
                    WorldState.HomeInventory().AddBuilding(new EssenceFilter());
                    break;
                case "Leather Plate":
                    WorldState.HomeInventory().Move(ArmourPlate.Create(ItemQuality.Rusted), 1);
                    break;
                case "Reinforced Leather Plate":
                    WorldState.HomeInventory().Move(ArmourPlate.Create(ItemQuality.Worn), 1);
                    break;
                case "Metal Plate":
                    WorldState.HomeInventory().Move(ArmourPlate.Create(ItemQuality.Shining), 1);
                    break;
                case "Alloy Plate":
                    WorldState.HomeInventory().Move(ArmourPlate.Create(ItemQuality.Radiant), 1);
                    break;
                case "Living Metal Plate":
                    WorldState.HomeInventory().Move(ArmourPlate.Create(ItemQuality.Faultless), 1);
                    break;
                case "Ice":
                    WorldState.HomeInventory().IncrementResource(ProductName, ProductQuantity);
                    break;
                case "Radiance":
                    WorldState.HomeInventory().IncrementResource(ProductName, 1);
                    break;
            }
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

                Recipe recipe = new Recipe(ingredient1Name, ingredient2Name, ingredient1Quantity, ingredient2Quantity, productName, productQuantity, 1);
                _recipes.Add(recipe);
            }

            _loaded = true;
        }
    }
}