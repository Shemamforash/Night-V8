using System.Collections.Generic;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

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
#if UNITY_EDITOR
        ResourceTemplate.AllResources.ForEach(r => { Inventory.IncrementResource(r.Name, Random.Range(5, 20)); });
        Inventory.IncrementResource("Essence", 38);
#endif
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
            bool canConsume = consumable.CanConsume();
            LeftText.SetStrikeThroughActive(!canConsume);
            CentreText.SetStrikeThroughActive(!canConsume);
            RightText.SetStrikeThroughActive(!canConsume);
            string nameText = consumable.Quantity() > 1 ? consumable.Name + " x" + consumable.Quantity() : consumable.Name;
            LeftText.SetText(nameText);
            CentreText.SetText("");
            RightText.SetText(consumable.Template.Description);
        }
    }

    private List<object> GetAvailableConsumables()
    {
        return Inventory.Consumables().ToObjectList();
    }

    public void Consume(object consumableObject)
    {
        Consumable consumable = (Consumable) consumableObject;
        consumable.Consume();
        UpdateCondition();
    }
}