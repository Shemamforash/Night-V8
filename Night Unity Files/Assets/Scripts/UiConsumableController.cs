using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine.UI;

public class UiConsumableController : UiGearMenuTemplate
{
    private EnhancedButton _consumableButton;
    private UIConditionController _thirstController, _hungerController;

    private void UpdateCondition()
    {
        _hungerController.UpdateHunger(CharacterManager.SelectedCharacter);
        _thirstController.UpdateThirst(CharacterManager.SelectedCharacter);
    }

    private void Awake()
    {
        _thirstController = Helper.FindChildWithName<UIConditionController>(gameObject, "Thirst");
        _hungerController = Helper.FindChildWithName<UIConditionController>(gameObject, "Hunger");
    }

    public override bool GearIsAvailable()
    {
        return WorldState.HomeInventory().Consumables().Count > 0;
    }

    public override void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi)
    {
        Consumable consumable = (Consumable) item;
        string nameText = consumable.Quantity() > 1 ? consumable.Name + " x" + consumable.Quantity() : consumable.Name;
        gearUi.SetTypeText(nameText);
        string effectText = consumable.Effect();
        gearUi.SetDpsText(effectText);
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
        return new List<MyGameObject>(WorldState.HomeInventory().Consumables());
    }

    public override void Equip(int selectedGear)
    {
        ((Consumable) GetAvailableGear()[selectedGear]).Consume(CharacterManager.SelectedCharacter);
        UpdateCondition();
    }

    public override Button GetGearButton()
    {
        throw new NotImplementedException();
    }
}