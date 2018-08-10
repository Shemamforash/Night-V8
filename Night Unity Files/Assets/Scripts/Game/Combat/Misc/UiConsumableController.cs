using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;

public class UiConsumableController : UiGearMenuTemplate
{
    private EnhancedButton _consumableButton;
    private UIConditionController _thirstController, _hungerController;
    private UIAttributeController _uiAttributeController;

    private void UpdateCondition()
    {
        _hungerController.UpdateHunger(CharacterManager.SelectedCharacter);
        _thirstController.UpdateThirst(CharacterManager.SelectedCharacter);
        _uiAttributeController.UpdateAttributes(CharacterManager.SelectedCharacter);
    }

    private void Awake()
    {
        _thirstController = gameObject.FindChildWithName<UIConditionController>("Thirst");
        _hungerController = gameObject.FindChildWithName<UIConditionController>("Hunger");
        _uiAttributeController = gameObject.FindChildWithName<UIAttributeController>("Attributes");
    }

    public override bool GearIsAvailable()
    {
        return UiGearMenuController.Inventory().Consumables().Count > 0;
    }

    public override void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi)
    {
        Consumable consumable = (Consumable) item;
        string nameText = consumable.Quantity() > 1 ? consumable.Name + " x" + consumable.Quantity() : consumable.Name;
        gearUi.SetTypeText(nameText);
        string effectText = consumable.Effect();
        gearUi.SetDpsText(effectText);
    }

    public override void Show()
    {
        base.Show();
        GetGearButton().Select();
        if (GearIsAvailable())
        {
            UiGearMenuController.EnableInput();
            UiGearMenuController.SelectGear();
        }

        UpdateCondition();
    }

    public override List<MyGameObject> GetAvailableGear()
    {
        return new List<MyGameObject>(UiGearMenuController.Inventory().Consumables());
    }

    public override void Equip(int selectedGear)
    {
        ((Consumable) GetAvailableGear()[selectedGear]).Consume(CharacterManager.SelectedCharacter);
        UpdateCondition();
    }
}