using System.Collections.Generic;
using System.Xml;
using DefaultNamespace;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Global;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class UiConsumableController : UiInventoryMenuController
{
    private static UIConditionController _thirstController;
    private static UIConditionController _hungerController;
    private static UIAttributeController _uiAttributeController;
    private ListController _consumableList;
    private static bool _unlocked;

    public static void Load(XmlNode root)
    {
        _unlocked = root.BoolFromNode("Consumable");
    }

    public static void Save(XmlNode root)
    {
        root.CreateChild("Consumable", _unlocked);
    }

    public override bool Unlocked()
    {
        if (!_unlocked) _unlocked = Inventory.Consumables().Count != 0;
        return _unlocked;
    }

    private static void UpdateConditions(Consumable consumable = null)
    {
        float hungerOffset = 0f;
        float waterOffset = 0f;
        float attributeOffset = 0;
        if (consumable != null)
        {
            hungerOffset = consumable.Template.ResourceType == ResourceType.Meat ? consumable.Template.EffectBonus : 0;
            waterOffset = consumable.Template.ResourceType == ResourceType.Water ? consumable.Template.EffectBonus : 0;
            if (consumable.Template.ResourceType == ResourceType.Plant) attributeOffset = consumable.Template.EffectBonus;
        }

        _hungerController.UpdateHunger(CharacterManager.SelectedCharacter, -hungerOffset);
        _thirstController.UpdateThirst(CharacterManager.SelectedCharacter, -waterOffset);
        if (attributeOffset == 0) _uiAttributeController.UpdateAttributes(CharacterManager.SelectedCharacter);
        else _uiAttributeController.UpdateAttributesOffset(CharacterManager.SelectedCharacter, consumable.Template.AttributeType, attributeOffset);
    }

    protected override void CacheElements()
    {
        _thirstController = gameObject.FindChildWithName<UIConditionController>("Thirst");
        _hungerController = gameObject.FindChildWithName<UIConditionController>("Hunger");
        _uiAttributeController = gameObject.FindChildWithName<UIAttributeController>("Attributes");
        _consumableList = gameObject.FindChildWithName<ListController>("List");
    }

    protected override void OnShow()
    {
        UiGearMenuController.SetCloseButtonAction(UiGearMenuController.Close);
        UpdateConditions();
        _consumableList.Show();
    }

    protected override void Initialise()
    {
        List<ListElement> listElements = new List<ListElement>();
        listElements.Add(new ConsumableElement());
        listElements.Add(new ConsumableElement());
        listElements.Add(new ConsumableElement());
        listElements.Add(new CentreConsumableElement());
        listElements.Add(new ConsumableElement());
        listElements.Add(new ConsumableElement());
        listElements.Add(new ConsumableElement());
        _consumableList.Initialise(listElements, Consume, UiGearMenuController.Close, GetAvailableConsumables);
    }

    private class CentreConsumableElement : ConsumableElement
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
            Consumable consumable = (Consumable) o;
            _descriptionText.SetText(consumable.Template.Description);
            UpdateConditions(consumable);
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

    private class ConsumableElement : ListElement
    {
        private EnhancedText _consumableText, _nameText, _effectText;
        private Image _icon;

        protected override void UpdateCentreItemEmpty()
        {
            _consumableText.SetText("");
            _nameText.SetText("No Consumables Available");
            _effectText.SetText("");
            _icon.SetAlpha(0f);
        }

        protected override void Update(object o, bool isCentreItem)
        {
            Consumable consumable = (Consumable) o;
            string nameText = consumable.Quantity() > 1 ? consumable.Name + " x" + consumable.Quantity() : consumable.Name;
            _nameText.SetText(nameText);
            _consumableText.SetText(consumable.CanConsume() ? "" : "Cannot Consume");
            _effectText.SetText(consumable.Template.EffectString);
            _icon.SetAlpha(1f);
            _icon.sprite = consumable.Template.Sprite;
        }

        protected override void SetVisible(bool visible)
        {
            _consumableText.gameObject.SetActive(visible);
            _nameText.gameObject.SetActive(visible);
            _effectText.gameObject.SetActive(visible);
            _icon.gameObject.SetActive(visible);
        }

        protected override void CacheUiElements(Transform transform)
        {
            _consumableText = transform.gameObject.FindChildWithName<EnhancedText>("Consumable");
            _nameText = transform.gameObject.FindChildWithName<EnhancedText>("Name");
            _effectText = transform.gameObject.FindChildWithName<EnhancedText>("Effect");
            _icon = transform.gameObject.FindChildWithName<Image>("Icon");
        }

        public override void SetColour(Color c)
        {
            _consumableText.SetColor(c);
            _nameText.SetColor(c);
            _effectText.SetColor(c);
            _icon.color = c;
        }
    }

    private List<object> GetAvailableConsumables()
    {
        return Inventory.Consumables().ToObjectList();
    }

    private void Consume(object consumableObject)
    {
        Consumable consumable = (Consumable) consumableObject;
        if (!consumable.CanConsume()) return;
        consumable.Consume();
        switch (consumable.Template.ResourceType)
        {
            case ResourceType.Water:
                UiGearMenuController.PlayAudio(AudioClips.EatWater);
                break;
            case ResourceType.Meat:
                UiGearMenuController.PlayAudio(AudioClips.EatMeat);
                break;
            case ResourceType.Plant:
                UiGearMenuController.PlayAudio(AudioClips.EatPlant);
                break;
            case ResourceType.Potion:
                UiGearMenuController.PlayAudio(AudioClips.EatPotion);
                break;
        }

        UpdateConditions();
    }
}