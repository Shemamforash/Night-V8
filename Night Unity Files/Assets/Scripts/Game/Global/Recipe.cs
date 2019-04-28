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
		private static readonly List<Recipe>    _recipes = new List<Recipe>();
		private static          bool            _loaded;
		private static          int             _craftingLevel;
		public readonly         string          Ingredient1,         Ingredient2, Name, Description;
		public readonly         int             Ingredient1Quantity, Ingredient2Quantity;
		public readonly         RecipeType      RecipeType;
		public readonly         RecipeAudioType RecipeAudio;
		private readonly        int             _levelNo;
		private                 bool            _canCraft;
		private readonly        int             _productQuantity;

		public enum RecipeAudioType
		{
			Cook,
			Craft,
			BoilWater,
			Furnace,
			LightFire
		}

		private Recipe(XmlNode recipeNode)
		{
			Ingredient1         = recipeNode.StringFromNode("Ingredient1Name");
			Ingredient2         = recipeNode.StringFromNode("Ingredient2Name");
			Ingredient1Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
			Ingredient2Quantity = recipeNode.IntFromNode("Ingredient1Quantity");
			Name                = recipeNode.StringFromNode("ProductName");
			Description         = recipeNode.StringFromNode("Description");
			_productQuantity    = recipeNode.IntFromNode("ProductQuantity");
			string recipeAudioString = recipeNode.StringFromNode("Audio");
			switch (recipeAudioString)
			{
				case "COOK":
					RecipeAudio = RecipeAudioType.Cook;
					break;
				case "CRAFT":
					RecipeAudio = RecipeAudioType.Craft;
					break;
				case "LIGHTFIRE":
					RecipeAudio = RecipeAudioType.LightFire;
					break;
				case "FURNACE":
					RecipeAudio = RecipeAudioType.Furnace;
					break;
				case "BOILWATER":
					RecipeAudio = RecipeAudioType.BoilWater;
					break;
			}

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

		private void CalculateQuantityToCraft()
		{
			int ingredient1Have = Inventory.GetResourceQuantity(Ingredient1);
			_canCraft = ingredient1Have >= Ingredient1Quantity;
			if (Ingredient2 == "None") return;
			int ingredient2Have = Inventory.GetResourceQuantity(Ingredient2);
			_canCraft = _canCraft && ingredient2Have >= Ingredient2Quantity;
		}

		public static void Save(XmlNode doc)
		{
			doc.CreateChild("CraftingLevel", _craftingLevel);
		}

		public static void Load(XmlNode doc)
		{
			_craftingLevel = doc.IntFromNode("CraftingLevel");
		}

		public static void RecalculateCraftableRecipes()
		{
			LoadRecipes();
			_recipes.ForEach(r => r.CalculateQuantityToCraft());
		}

		public bool CanCraft() => _canCraft;


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
					Inventory.IncrementResource(Name, _productQuantity);
					break;
				case RecipeType.Fire:
					break;
				case RecipeType.Upgrade:
					++_craftingLevel;
					break;
			}

			RecalculateCraftableRecipes();
		}

		private bool Available()
		{
			bool validLevel = _levelNo <= (int) EnvironmentManager.CurrentEnvironmentType;
			if (!validLevel) return false;
			if (RecipeType == RecipeType.Fire) return true;
			if (!Campfire.IsLit()) return false;
			if (RecipeType        == RecipeType.Upgrade) return _levelNo == _craftingLevel + 1;
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

		public int GetProductQuantity() => _productQuantity;

		public void RestoreResources()
		{
			Inventory.IncrementResource(Ingredient1, Ingredient1Quantity);
			if (Ingredient2 != "None") Inventory.IncrementResource(Ingredient2, Ingredient2Quantity);
		}
	}
}