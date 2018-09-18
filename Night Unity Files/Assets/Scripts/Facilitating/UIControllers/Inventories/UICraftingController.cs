using System.Collections.Generic;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;

public class UICraftingController : UiInventoryMenuController, IInputListener
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
        InputHandler.RegisterInputListener(this);
        if (PlayerCombat.Instance != null)
        {
            _buildButton.enabled = false;
            _buildButton.GetComponent<EnhancedText>().SetStrikeThroughActive(true);    
        }
        else
        {
            _buildButton.enabled = true;
            _buildButton.GetComponent<EnhancedText>().SetStrikeThroughActive(false);    
        }
    }

    protected override void Initialise()
    {
        _buildingsList.Initialise(typeof(RecipeElement), CreateRecipe, CloseRecipeList);
        _buildButton.AddOnClick(() =>
        {
            InputHandler.UnregisterInputListener(this);
            _buildingsList.Show(GetAvailableRecipes);
            _builtText.gameObject.SetActive(false);
            _buildButton.gameObject.SetActive(false);
        });
    }

    protected override void OnHide()
    {
        InputHandler.UnregisterInputListener(this);
    }

    private void CloseRecipeList()
    {
        _buildingsList.Hide();
        _builtText.gameObject.SetActive(true);
        _buildButton.gameObject.SetActive(true);
        _buildButton.Select();
    }

    private void CreateRecipe(object obj)
    {
        Recipe recipe = (Recipe) obj;
        CharacterManager.SelectedCharacter.CraftAction.StartCrafting(recipe);
        UiGearMenuController.Close();
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
            LeftText.SetText("");
            CentreText.SetText(productString);
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

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || axis != InputAxis.Cover) return;
        UiGearMenuController.Close();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}