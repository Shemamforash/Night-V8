using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine.UI;

public class UICraftingController : UiGearMenuTemplate
{
    private Player _player;
    private EnhancedButton _closeButton;
    private EnhancedButton _recipeButton;

    public void Awake()
    {
//        base.Awake();
//        _instance = this;
//        Initialise();
    }

    public override bool GearIsAvailable()
    {
        return Recipe.Recipes().Count > 0;
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
            if (recipe.Ingredient1 == "") ingredient1String = "";
            string ingredient2String = recipe.Ingredient2Quantity > 1 ? recipe.Ingredient2 + " x" + recipe.Ingredient2Quantity : recipe.Ingredient2;
            if (recipe.Ingredient2 == "") ingredient2String = "";
            gearUi.SetDpsText(ingredient1String + " " + ingredient2String);
            return;
        }

        gearUi.SetTypeText("");
        gearUi.SetNameText("");
        gearUi.SetDpsText("");
    }

    public override void CompareTo(MyGameObject comparisonItem)
    {
        throw new NotImplementedException();
    }

    public override void StopComparing()
    {
        throw new NotImplementedException();
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

    public override Button GetGearButton()
    {
        return null;
    }
}