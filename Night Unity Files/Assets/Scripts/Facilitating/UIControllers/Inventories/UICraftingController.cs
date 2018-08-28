using System.Collections.Generic;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;

public class UICraftingController : UiInventoryMenuController
{
    private EnhancedButton _buildButton;
    private EnhancedText _builtText;
    private ListController _buildingsList;

    protected override void CacheElements()
    {
        _builtText = gameObject.FindChildWithName<EnhancedText>("Built");
        _buildingsList = gameObject.FindChildWithName<ListController>("List");
        _buildButton = gameObject.FindChildWithName<EnhancedButton>("Build");
    }

    protected override void OnShow()
    {
        DisplayBuildings();
        CloseRecipeList();
    }

    protected override void Initialise()
    {
        _buildingsList.Initialise(typeof(RecipeElement), CreateRecipe, CloseRecipeList);
        _buildButton.AddOnClick(() => { _buildingsList.Show(GetAvailableRecipes); });
    }

    private void CloseRecipeList()
    {
        _buildingsList.Hide();
        _buildButton.Select();
    }

    private void CreateRecipe(object obj)
    {
        Recipe recipe = (Recipe) obj;
        recipe.Craft();
        _buildingsList.Hide();
    }

    private class RecipeElement : BasicListElement
    {
        protected override void UpdateCentreItemEmpty()
        {
            LeftText.SetText("");
            CentreText.SetText("No Recipes Unlocked");
            RightText.SetText("");
        }

        protected override void Update(object o)
        {
            Recipe recipe = (Recipe) o;
            string productString = recipe.ProductQuantity > 1 ? recipe.ProductName + " x" + recipe.ProductQuantity : recipe.ProductName;
            string durationString = WorldState.TimeToHours((int) (recipe.DurationInHours * WorldState.MinutesPerHour));
            productString += " (" + durationString + ")";
            LeftText.SetText(productString);
            CentreText.SetText("");
            string ingredient1String = recipe.Ingredient1Quantity > 1 ? recipe.Ingredient1 + " x" + recipe.Ingredient1Quantity : recipe.Ingredient1;
            if (recipe.Ingredient1 == "None") ingredient1String = "";
            string ingredient2String = recipe.Ingredient2Quantity > 1 ? recipe.Ingredient2 + " x" + recipe.Ingredient2Quantity : recipe.Ingredient2;
            if (recipe.Ingredient2 == "None") ingredient2String = "";
            RightText.SetText(ingredient1String + " " + ingredient2String);

            bool canAfford = recipe.CanCraft();
            CentreText.SetStrikeThroughActive(!canAfford);
            LeftText.SetStrikeThroughActive(!canAfford);
            RightText.SetStrikeThroughActive(!canAfford);
        }
    }

    private void DisplayBuildings()
    {
        List<Building> buildings = WorldState.HomeInventory().Buildings();
        if (buildings.Count == 0)
        {
            _builtText.SetText("Nothing built");
            return;
        }

        string buildingString = "";
        for (int i = 0; i < buildings.Count; ++i)
        {
            buildingString += buildings[i].Name;
            if (i < buildings.Count - 1) buildingString += "\n";
        }

        _builtText.SetText(buildingString);
    }

    private static List<object> GetAvailableRecipes()
    {
        return Recipe.Recipes().ToObjectList();
    }
}