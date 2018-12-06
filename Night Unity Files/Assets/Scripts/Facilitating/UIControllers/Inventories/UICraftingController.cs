using System.Collections.Generic;
using DG.Tweening;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UICraftingController : UiInventoryMenuController, IInputListener
{
    private ListController _craftingList;
    private CanvasGroup _listCanvas, _currentCraftingCanvas;
    private EnhancedButton _acceptButton;
    private EnhancedText _currentCraftingName;
    private ColourPulse _glow;
    private static bool _unlocked;

    protected override void SetUnlocked(bool unlocked) => _unlocked = unlocked;

    public override bool Unlocked()
    {
        if (!_unlocked) _unlocked = Recipe.RecipesAvailable();
        return _unlocked;
    }
    
    protected override void CacheElements()
    {
        _craftingList = gameObject.FindChildWithName<ListController>("List");
        _listCanvas = _craftingList.GetComponent<CanvasGroup>();
        _currentCraftingCanvas = gameObject.FindChildWithName<CanvasGroup>("Currently Crafting");
        _currentCraftingName = _currentCraftingCanvas.gameObject.FindChildWithName<EnhancedText>("Name");
        _glow = _currentCraftingCanvas.gameObject.FindChildWithName<ColourPulse>("Title Glow");
        _acceptButton = _currentCraftingCanvas.gameObject.FindChildWithName<EnhancedButton>("Accept");
        _acceptButton.AddOnClick(() =>
        {
            UiGearMenuController.Close();
            _glow.SetAlphaMultiplier(1f);
            DOTween.To(_glow.GetAlphaMultiplier, _glow.SetAlphaMultiplier, 0f, 0.5f);
        });
    }

    protected override void OnShow()
    {
        UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
        InputHandler.RegisterInputListener(this);
        if (CharacterManager.SelectedCharacter.CraftAction.IsCurrentState()) ShowCurrentlyCrafting();
        else ShowCraftingList();
        TutorialManager.TryOpenTutorial(10);
    }

    private void ShowCurrentlyCrafting()
    {
        _listCanvas.alpha = 0;
        _currentCraftingCanvas.alpha = 0;
        _currentCraftingCanvas.DOFade(1, 0.5f).SetUpdate(UpdateType.Normal, true);
        _currentCraftingName.SetText(CharacterManager.SelectedCharacter.CraftAction.GetRecipeName());
        _glow.SetAlphaMultiplier(0.5f);
        _acceptButton.Select();
    }

    private void ShowCraftingList()
    {
        _listCanvas.alpha = 1;
        _currentCraftingCanvas.alpha = 0;
        _craftingList.Show();
    }

    protected override void Initialise()
    {
        _craftingList.Initialise(typeof(RecipeElement), CreateRecipe, UiGearMenuController.Close, GetAvailableRecipes);
    }

    protected override void OnHide()
    {
        InputHandler.UnregisterInputListener(this);
    }

    private void CreateRecipe(object obj)
    {
        Recipe recipe = (Recipe) obj;
        if (!recipe.CanCraft()) return;
        if (!CharacterManager.SelectedCharacter.TravelAction.AtHome()) return;
        CharacterManager.SelectedCharacter.CraftAction.StartCrafting(recipe);
        ShowCurrentlyCrafting();
        UiGearMenuController.PlayAudio(AudioClips.Craft);
    }

    private class RecipeElement : BasicListElement
    {
        protected override void UpdateCentreItemEmpty()
        {
            LeftText.SetText("");
            CentreText.SetText("No Recipes Unlocked");
            RightText.SetText("");
        }

        protected override void Update(object o, bool isCentreItem)
        {
            Recipe recipe = (Recipe) o;
            string productString = recipe.ProductQuantity > 1 ? recipe.Name + " x" + recipe.ProductQuantity : recipe.Name;
            if (recipe.RecipeType == RecipeType.Building) productString += " (Built " + recipe.Built() + ")";
            LeftText.SetText(productString);

            bool canAfford = recipe.CanCraft();
            bool canCraft = CharacterManager.SelectedCharacter.TravelAction.AtHome();
            string affordString = "";
            if (!canCraft) affordString += "Cannot Craft When Travelling";
            else if (!canAfford) affordString += "Insufficient Resources";
            CentreText.SetText(affordString);

            string ingredient1String = recipe.Ingredient1Quantity > 1 ? recipe.Ingredient1 + " x" + recipe.Ingredient1Quantity : recipe.Ingredient1;
            if (recipe.Ingredient1 == "None") ingredient1String = "";
            string ingredient2String = recipe.Ingredient2Quantity > 1 ? recipe.Ingredient2 + " x" + recipe.Ingredient2Quantity : recipe.Ingredient2;
            if (recipe.Ingredient2 == "None") ingredient2String = "";
            RightText.SetText(ingredient1String + " " + ingredient2String);
        }
    }

    private static List<object> GetAvailableRecipes()
    {
        return Recipe.Recipes().ToObjectList();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || axis != InputAxis.Menu) return;
        UiGearMenuController.Close();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}