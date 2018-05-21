using System;
using System.Collections.Generic;
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
    private const int centre = 5;
    private int _selectedRecipe;
    private readonly List<RecipeUi> _recipeUis = new List<RecipeUi>();

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        Initialise();
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
                Recipe.Recipes()[_selectedRecipe].Craft();
                break;
            case InputAxis.Reload:
                InputHandler.SetCurrentListener(CombatManager.Player());
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
        if (_selectedRecipe == Recipe.Recipes().Count - 1) return;
        ++_selectedRecipe;
        SelectItem();
    }

    private void TrySelectItemAbove()
    {
        if (_selectedRecipe == 0) return;
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
    }

    private void Initialise()
    {
        for (int i = 0; i < 9; ++i)
        {
            GameObject uiObject = Helper.FindChildWithName(gameObject, "Recipe " + i);
            RecipeUi recipeUi = new RecipeUi(uiObject, Math.Abs(i - centre));
            _recipeUis.Add(recipeUi);
            recipeUi.SetRecipe(null);
        }
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
            _ingredient1Text = Helper.FindChildWithName<EnhancedText>(uiObject, "Ingredient 1");
            _ingredient1Text = Helper.FindChildWithName<EnhancedText>(uiObject, "Ingredient 2");
            _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
        }

        private void SetColour(Color c)
        {
            _ingredient1Text.SetColor(c);
            _productText.SetColor(c);
            _durationText.SetColor(c);
        }

        private void SetDurationText(string text)
        {
            _durationText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
            _durationText.Text(text);
        }

        private void SetProductText(string text)
        {
            _productText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
            _productText.Text(text);
        }

        private void SetIngredient1Text(InventoryResourceType type, int quantity)
        {
            _ingredient1Text.SetColor(type == InventoryResourceType.None ? UiAppearanceController.InvisibleColour : _activeColour);
            _ingredient1Text.Text(type + " x" + quantity);
        }

        private void SetIngredient2Text(InventoryResourceType type, int quantity)
        {
            _ingredient2Text.SetColor(type == InventoryResourceType.None ? UiAppearanceController.InvisibleColour : _activeColour);
            _ingredient2Text.Text(type + " x" + quantity);
        }

        public void SetRecipe(Recipe recipe)
        {
            if (recipe == null)
            {
                SetColour(UiAppearanceController.InvisibleColour);
                return;
            }

            SetDurationText(recipe.DurationInHours * WorldState.MinutesPerHour + " mins");
            SetProductText(recipe.Product.Name);
            SetIngredient1Text(recipe.Ingredient1, recipe.Ingredient1Quantity);
            SetIngredient2Text(recipe.Ingredient2, recipe.Ingredient2Quantity);
        }
    }
}
