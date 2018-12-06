using System.Collections.Generic;
using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Global;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using Random = UnityEngine.Random;

public class UiConsumableController : UiInventoryMenuController
{
    private static UIConditionController _thirstController;
    private static UIConditionController _hungerController;
    private static UIAttributeController _uiAttributeController;
    private ListController _consumableList;
    private static bool _unlocked;

    protected override void SetUnlocked(bool unlocked) => _unlocked = unlocked;

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
#if UNITY_EDITOR
        ResourceTemplate.AllResources.ForEach(r => { Inventory.IncrementResource(r.Name, Random.Range(5, 20)); });
        Inventory.IncrementResource("Essence", 38);
#endif
        _consumableList.Show();
    }

    protected override void Initialise()
    {
        _consumableList.Initialise(typeof(ConsumableElement), Consume, UiGearMenuController.Close, GetAvailableConsumables);
    }

    private class ConsumableElement : BasicListElement
    {
        protected override void UpdateCentreItemEmpty()
        {
            LeftText.SetText("");
            CentreText.SetText("No Consumables Available");
            RightText.SetText("");
        }

        protected override void Update(object o, bool isCentreItem)
        {
            Consumable consumable = (Consumable) o;
            string nameText = consumable.Quantity() > 1 ? consumable.Name + " x" + consumable.Quantity() : consumable.Name;
            LeftText.SetText(nameText);
            CentreText.SetText(consumable.CanConsume() ? "" : "Cannot Consume");
            RightText.SetText(consumable.Template.Description);
            if (isCentreItem) UpdateConditions(consumable);
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