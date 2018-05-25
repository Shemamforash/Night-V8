using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UICraftingController : Menu, IInputListener
{
    private static UICraftingController _instance;
    private const int centre = 4;
    private int _selectedRecipe;
    private readonly List<RecipeUi> _recipeUis = new List<RecipeUi>();
    private Player _player;
    private EnhancedButton _closeButton;
    private EnhancedButton _recipeButton;
    private bool _focussed;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        Initialise();
    }

    public static void ShowMenu()
    {
        MenuStateMachine.ShowMenu("Crafting Menu");
        _instance.SelectItem();
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public override void Enter()
    {
        base.Enter();
        InputHandler.SetCurrentListener(this);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        switch (axis)
        {
            case InputAxis.Vertical:
                if (direction < 0)
                    TrySelectItemBelow();
                else
                    TrySelectItemAbove();
                break;
            case InputAxis.Fire:
                if (_focussed && Recipe.Recipes()[_selectedRecipe].Craft())
                {
                    MenuStateMachine.ReturnToDefault();
                }

                break;
            case InputAxis.Reload:
                MenuStateMachine.ReturnToDefault();
                break;
        }
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    private void TrySelectItemBelow()
    {
        if (_selectedRecipe == Recipe.Recipes().Count - 1)
        {
            _focussed = false;
            _closeButton.Select();
            return;
        }

        ++_selectedRecipe;
        SelectItem();
    }

    private void TrySelectItemAbove()
    {
        if (_selectedRecipe == 0)
        {
            _focussed = false;
            _closeButton.Select();
            return;
        }

        --_selectedRecipe;
        SelectItem();
    }

    private void SelectItem()
    {
        List<Recipe> recipes = Recipe.Recipes();
        if (_selectedRecipe >= recipes.Count) _selectedRecipe = recipes.Count - 1;
        for (int i = 0; i < _recipeUis.Count; ++i)
        {
            int offset = i - centre;
            int targetRecipeIndex = _selectedRecipe + offset;
            Recipe recipe = null;
            if (targetRecipeIndex >= 0 && targetRecipeIndex < recipes.Count) recipe = recipes[targetRecipeIndex];
            _recipeUis[i].SetRecipe(recipe);
        }

        _recipeButton.Select();
        _focussed = true;
    }

    private void Initialise()
    {
        for (int i = 0; i < 9; ++i)
        {
            GameObject uiObject = Helper.FindChildWithName(gameObject, "Recipe " + i);
            RecipeUi recipeUi = new RecipeUi(uiObject, Math.Abs(i - centre));
            if (i == centre)
            {
                _recipeButton = uiObject.GetComponent<EnhancedButton>();
            }

            _recipeUis.Add(recipeUi);
            recipeUi.SetRecipe(null);
        }

        _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Close");
        _closeButton.AddOnClick(MenuStateMachine.ReturnToDefault);
        _closeButton.SetOnUpAction(SelectLast);
        _closeButton.SetOnDownAction(SelectFirst);
    }

    private void SelectLast()
    {
        _selectedRecipe = Recipe.Recipes().Count - 1;
        SelectItem();
    }

    private void SelectFirst()
    {
        _selectedRecipe = 0;
        SelectItem();
    }

    private class RecipeUi
    {
        private readonly Color _activeColour;
        private readonly EnhancedText _durationText;
        private readonly EnhancedText _productText;
        private readonly EnhancedText _ingredient1Text;
        private readonly EnhancedText _ingredient2Text;

        public RecipeUi(GameObject uiObject, int offset)
        {
            _productText = Helper.FindChildWithName<EnhancedText>(uiObject, "Name");
            _durationText = Helper.FindChildWithName<EnhancedText>(uiObject, "Duration");
            _ingredient1Text = Helper.FindChildWithName<EnhancedText>(uiObject, "Ingredient 2");
            _ingredient2Text = Helper.FindChildWithName<EnhancedText>(uiObject, "Ingredient 1");
            _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
        }

        private void SetColour(Color c)
        {
            _ingredient1Text.SetColor(c);
            _ingredient2Text.SetColor(c);
            _productText.SetColor(c);
            _durationText.SetColor(c);
        }

        private void SetDurationText(string text)
        {
            _durationText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
            _durationText.Text(text);
        }

        private void SetProductText(string text, int quantity)
        {
            _productText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
            if (quantity > 1) text += " x" + quantity;
            _productText.Text(text);
        }

        private void SetIngredient1Text(InventoryResourceType type, int quantity)
        {
            _ingredient1Text.SetColor(type == InventoryResourceType.None ? UiAppearanceController.InvisibleColour : _activeColour);
            string text = type.ToString();
            if (quantity > 1) text += " x" + quantity;
            _ingredient1Text.Text(text);
        }

        private void SetIngredient2Text(InventoryResourceType type, int quantity)
        {
            _ingredient2Text.SetColor(type == InventoryResourceType.None ? UiAppearanceController.InvisibleColour : _activeColour);
            string text = type.ToString();
            if (quantity > 1) text += " x" + quantity;
            _ingredient2Text.Text(text);
        }

        public void SetRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                SetColour(UiAppearanceController.InvisibleColour);
                return;
            }

            SetDurationText(WorldState.TimeToHours((int) (recipe.DurationInHours * WorldState.MinutesPerHour)));
            SetProductText(recipe.ProductName, recipe.ProductQuantity);
            SetIngredient1Text(recipe.Ingredient1, recipe.Ingredient1Quantity);
            SetIngredient2Text(recipe.Ingredient2, recipe.Ingredient2Quantity);
        }
    }
}