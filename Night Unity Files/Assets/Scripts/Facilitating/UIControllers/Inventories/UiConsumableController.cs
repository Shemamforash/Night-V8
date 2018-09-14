﻿using System.Collections.Generic;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;

public class UiConsumableController : UiInventoryMenuController
{
    private UIConditionController _thirstController, _hungerController;
    private UIAttributeController _uiAttributeController;
    private ListController _consumableList;

    private void UpdateCondition()
    {
        _hungerController.UpdateHunger(CharacterManager.SelectedCharacter);
        _thirstController.UpdateThirst(CharacterManager.SelectedCharacter);
        _uiAttributeController.UpdateAttributes(CharacterManager.SelectedCharacter);
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
        UpdateCondition();
        _consumableList.Show(GetAvailableConsumables);
    }

    protected override void Initialise()
    {
        _consumableList.Initialise(typeof(ConsumableElement), Consume, UiGearMenuController.Close);
    }

    private class ConsumableElement : BasicListElement
    {
        protected override void UpdateCentreItemEmpty()
        {
            LeftText.SetText("");
            CentreText.SetText("No Consumables Available");
            RightText.SetText("");
        }

        protected override void Update(object o)
        {
            Consumable consumable = (Consumable) o;
            string nameText = consumable.Quantity() > 1 ? consumable.Name + " x" + consumable.Quantity() : consumable.Name;
            LeftText.SetText(nameText);
            CentreText.SetText("");
            RightText.SetText(consumable.Effect());
        }
    }

    private List<object> GetAvailableConsumables()
    {
        return UiGearMenuController.Inventory().Consumables().ToObjectList();
    }

    public void Consume(object consumableObject)
    {
        Consumable consumable = (Consumable) consumableObject;
        consumable.Consume(CharacterManager.SelectedCharacter);
        UpdateCondition();
    }
}