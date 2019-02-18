using System.Collections.Generic;
using System.Xml;
using DefaultNamespace;
using DG.Tweening;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using Game.Global;
using Game.Global.Tutorial;
using SamsHelper.BaseGameFunctionality.InventorySystem;
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
    private List<TutorialOverlay> _overlays;
    private bool _seenTutorial;

    public static void Load(XmlNode root)
    {
        _unlocked = root.BoolFromNode("Crafting");
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Crafting", _unlocked);
    }

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


        ShowCraftingTutorial();
    }

    private void ShowCraftingTutorial()
    {
        if (_seenTutorial || !TutorialManager.Active()) return;
        List<TutorialOverlay> overlays = new List<TutorialOverlay>
        {
            new TutorialOverlay(ResourcesUiController.ResourceRect()),
            new TutorialOverlay()
        };
        TutorialManager.TryOpenTutorial(11, overlays, false);
        _seenTutorial = true;
    }

    private void ShowCurrentlyCrafting()
    {
        _currentCraftingCanvas.gameObject.SetActive(true);
        _listCanvas.gameObject.SetActive(false);

        _currentCraftingCanvas.alpha = 0;
        _currentCraftingCanvas.DOFade(1, 0.5f).SetUpdate(UpdateType.Normal, true);

        _currentCraftingName.SetText(CharacterManager.SelectedCharacter.CraftAction.GetRecipeName());
        _glow.SetAlphaMultiplier(0.5f);
        _acceptButton.Select();
    }

    private void ShowCraftingList()
    {
        _currentCraftingCanvas.gameObject.SetActive(false);
        _listCanvas.gameObject.SetActive(true);

        _listCanvas.alpha = 0;
        _listCanvas.DOFade(1, 0.5f).SetUpdate(UpdateType.Normal, true);

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
        UiGearMenuController.PlayAudio(recipe.RecipeType == RecipeType.Fire ? AudioClips.LightFire : AudioClips.Craft);
    }

    private class RecipeElement : ListElement
    {
        private EnhancedText LeftText;
        private EnhancedText RightText;

        protected override void SetVisible(bool visible)
        {
            LeftText.gameObject.SetActive(visible);
            RightText.gameObject.SetActive(visible);
        }

        protected override void CacheUiElements(Transform transform)
        {
            LeftText = transform.gameObject.FindChildWithName<EnhancedText>("Type");
            RightText = transform.gameObject.FindChildWithName<EnhancedText>("Dps");
        }

        public override void SetColour(Color c)
        {
            LeftText.SetColor(c);
            RightText.SetColor(c);
        }

        protected override void UpdateCentreItemEmpty()
        {
            LeftText.SetText("");
            RightText.SetText("");
        }

        protected override void Update(object o, bool isCentreItem)
        {
            Recipe recipe = (Recipe) o;
            string productString = recipe.GetCraftableQuantity() > 1 ? recipe.Name + " x" + recipe.GetCraftableQuantity() : recipe.Name;
            if (recipe.RecipeType == RecipeType.Building) productString += " (Built " + recipe.Built() + ")";
            LeftText.SetText(productString);
            string ingredient1String = GetIngredientString(recipe.Ingredient1, recipe.Ingredient1Quantity);
            string ingredient2String = GetIngredientString(recipe.Ingredient2, recipe.Ingredient2Quantity);
            string fullString = ingredient1String;
            if (ingredient2String != "") fullString += "  -  " + ingredient2String;
            RightText.SetText(fullString);
        }

        private string GetIngredientString(string ingredientName, int requiredQuantity)
        {
            if (ingredientName == "None") return "";
            int currentQuantity = Inventory.GetResourceQuantity(ingredientName);
            return currentQuantity + "/" + requiredQuantity + " " + ingredientName;
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