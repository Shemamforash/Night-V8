using System.Collections.Generic;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;

public class UICraftingController : UiInventoryMenuController, IInputListener
{
    private ListController _buildingsList;

    protected override void CacheElements()
    {
        _buildingsList = gameObject.FindChildWithName<ListController>("List");
    }

    protected override void OnShow()
    {
        InputHandler.RegisterInputListener(this);
        _buildingsList.Show(GetAvailableRecipes);
    }

    protected override void Initialise()
    {
        _buildingsList.Initialise(typeof(RecipeElement), CreateRecipe, UiGearMenuController.Close);
    }

    protected override void OnHide()
    {
        InputHandler.UnregisterInputListener(this);
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
            string durationString = WorldState.TimeToHours((int) (Recipe.DurationInHours * WorldState.MinutesPerHour));
            productString += " (" + durationString + ")";
            LeftText.SetText("");

            if (recipe.IsBuilding) LeftText.SetText("Built " + recipe.Built());

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