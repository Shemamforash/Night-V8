﻿using System;
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
using UnityEngine.UI;

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
        List<ListElement> listElements = new List<ListElement>();
        listElements.Add(new CraftingElement());
        listElements.Add(new CraftingElement());
        listElements.Add(new CraftingElement());
        listElements.Add(new CentreCraftingElement());
        listElements.Add(new CraftingElement());
        listElements.Add(new CraftingElement());
        listElements.Add(new CraftingElement());
        _craftingList.Initialise(listElements, CreateRecipe, UiGearMenuController.Close, GetAvailableRecipes);
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
        UiGearMenuController.Close();
        AudioClip audioClip;
        switch (recipe.RecipeAudio)
        {
            case Recipe.RecipeAudioType.Cook:
                audioClip = AudioClips.CookMeat;
                break;
            case Recipe.RecipeAudioType.Craft:
                audioClip = AudioClips.Craft;
                break;
            case Recipe.RecipeAudioType.BoilWater:
                audioClip = AudioClips.BoilWater;
                break;
            case Recipe.RecipeAudioType.Furnace:
                audioClip = AudioClips.Furnace;
                break;
            case Recipe.RecipeAudioType.LightFire:
                audioClip = AudioClips.LightFire;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        UiGearMenuController.PlayAudio(audioClip);
    }


    private class CentreCraftingElement : CraftingElement
    {
        private EnhancedText _descriptionText;

        protected override void UpdateCentreItemEmpty()
        {
            base.UpdateCentreItemEmpty();
            _descriptionText.SetText("-");
        }

        protected override void Update(object o, bool isCentreItem)
        {
            base.Update(o, isCentreItem);
            Recipe recipe = (Recipe) o;
            _descriptionText.SetText(recipe.Description);
        }

        protected override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            _descriptionText.gameObject.SetActive(visible);
        }

        protected override void CacheUiElements(Transform transform)
        {
            base.CacheUiElements(transform);
            _descriptionText = transform.gameObject.FindChildWithName<EnhancedText>("Description");
        }

        public override void SetColour(Color c)
        {
            base.SetColour(c);
            c.a = 0.6f;
            _descriptionText.SetColor(c);
        }
    }

    private class CraftingElement : ListElement
    {
        private EnhancedText _craftableText, _nameText, _costText;
        private Image _icon;

        protected override void UpdateCentreItemEmpty()
        {
            _craftableText.SetText("");
            _nameText.SetText("No Recipes Available");
            _costText.SetText("");
            _icon.SetAlpha(0f);
        }

        protected override void Update(object o, bool isCentreItem)
        {
            Recipe recipe = (Recipe) o;
            string productString = recipe.GetProductQuantity() > 1 ? recipe.Name + " x" + recipe.GetProductQuantity() : recipe.Name;
            _nameText.SetText(productString);

            string craftableString = "";
            if (recipe.RecipeType == RecipeType.Building)
            {
                int builtCount = recipe.Built();
                if (builtCount != 0) craftableString += "Built " + recipe.Built() + "   ";
            }

            if (!recipe.CanCraft()) craftableString += "Need Resources";
            _craftableText.SetText(craftableString);

            string ingredient1String = GetIngredientString(recipe.Ingredient1, recipe.Ingredient1Quantity);
            string ingredient2String = GetIngredientString(recipe.Ingredient2, recipe.Ingredient2Quantity);
            string fullString = ingredient1String;
            if (ingredient2String != "") fullString += "\n" + ingredient2String;
            _costText.SetText(fullString);
        }

        private string GetIngredientString(string ingredientName, int requiredQuantity)
        {
            if (ingredientName == "None") return "";
            int currentQuantity = Inventory.GetResourceQuantity(ingredientName);
            return currentQuantity + "/" + requiredQuantity + " " + ingredientName;
        }

        protected override void SetVisible(bool visible)
        {
            _craftableText.gameObject.SetActive(visible);
            _nameText.gameObject.SetActive(visible);
            _costText.gameObject.SetActive(visible);
            _icon.gameObject.SetActive(visible);
        }

        protected override void CacheUiElements(Transform transform)
        {
            _craftableText = transform.gameObject.FindChildWithName<EnhancedText>("Craftable");
            _nameText = transform.gameObject.FindChildWithName<EnhancedText>("Name");
            _costText = transform.gameObject.FindChildWithName<EnhancedText>("Cost");
            _icon = transform.gameObject.FindChildWithName<Image>("Icon");
        }

        public override void SetColour(Color c)
        {
            _craftableText.SetColor(c);
            _nameText.SetColor(c);
            _costText.SetColor(c);
            _icon.color = c;
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