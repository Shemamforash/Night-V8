using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;

public class UICraftingController : UiGearMenuTemplate
{
    private Player _player;
    private EnhancedButton _closeButton;
    private EnhancedButton _recipeButton;
    private EnhancedText _builtText;

    public override bool GearIsAvailable()
    {
        return Recipe.Recipes().Count > 0;
    }

    public void Awake()
    {
        _builtText = gameObject.FindChildWithName<EnhancedText>("Built");
    }

    public override void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi)
    {
        Recipe recipe = (Recipe) item;
        if (recipe != null)
        {
            string productString = recipe.ProductQuantity > 1 ? recipe.ProductName + " x" + recipe.ProductQuantity : recipe.ProductName;
            string durationString = WorldState.TimeToHours((int) (recipe.DurationInHours * WorldState.MinutesPerHour));
            productString += " (" + durationString + ")";
            gearUi.SetTypeText(productString);
            gearUi.SetNameText("");
            string ingredient1String = recipe.Ingredient1Quantity > 1 ? recipe.Ingredient1 + " x" + recipe.Ingredient1Quantity : recipe.Ingredient1;
            if (recipe.Ingredient1 == "None") ingredient1String = "";
            string ingredient2String = recipe.Ingredient2Quantity > 1 ? recipe.Ingredient2 + " x" + recipe.Ingredient2Quantity : recipe.Ingredient2;
            if (recipe.Ingredient2 == "None") ingredient2String = "";
            gearUi.SetDpsText(ingredient1String + " " + ingredient2String);
            return;
        }

        gearUi.SetTypeText("");
        gearUi.SetNameText("");
        gearUi.SetDpsText("");
    }

    public override void Show()
    {
        base.Show();
        MenuStateMachine.SelectInactiveButton(GetGearButton());
        DisplayBuildings();
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

    public override List<MyGameObject> GetAvailableGear()
    {
        return new List<MyGameObject>(Recipe.Recipes());
    }

    public override void Equip(int selectedGear)
    {
        if (Recipe.Recipes()[selectedGear].Craft())
        {
            MenuStateMachine.ReturnToDefault();
        }
    }
}