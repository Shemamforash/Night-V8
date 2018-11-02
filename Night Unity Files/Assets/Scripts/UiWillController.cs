﻿using Facilitating.UIControllers;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;

public class UiWillController : UiInventoryMenuController, IInputListener
{
    private UIAttributeController _uiAttributeController;
    private EnhancedButton _fettleButton, _gritButton, _focusButton;

    protected override void CacheElements()
    {
        _uiAttributeController = GetComponent<UIAttributeController>();
        _fettleButton = gameObject.FindChildWithName("Fettle").transform.FindChildWithName("Text").GetComponent<EnhancedButton>();
        _gritButton = gameObject.FindChildWithName("Grit").transform.FindChildWithName("Text").GetComponent<EnhancedButton>();
        _focusButton = gameObject.FindChildWithName("Focus").transform.FindChildWithName("Text").GetComponent<EnhancedButton>();
    }

    protected override void OnShow()
    {
        InputHandler.RegisterInputListener(this);
        UpdateValues();
        _fettleButton.Select();
    }

    protected override void OnHide()
    {
        InputHandler.UnregisterInputListener(this);
    }

    protected override void Initialise()
    {
        _fettleButton.AddOnClick(() => TryRestoreAttribute(AttributeType.Fettle));
        _gritButton.AddOnClick(() => TryRestoreAttribute(AttributeType.Grit));
        _focusButton.AddOnClick(() => TryRestoreAttribute(AttributeType.Focus));
    }

    private void UpdateValues()
    {
        _uiAttributeController.UpdateAttributes(CharacterManager.SelectedCharacter);
    }

    private void TryRestoreAttribute(AttributeType attributeType)
    {
        CharacterAttributes attributes = CharacterManager.SelectedCharacter.Attributes;
        CharacterAttribute will = attributes.Get(AttributeType.Will);
        CharacterAttribute targetAttribute = attributes.Get(attributeType);
        if (targetAttribute.ReachedMax()) return;
        if (will.ReachedMin()) return;
        will.Decrement();
        targetAttribute.Increment();
        UpdateValues();
        if (attributeType != AttributeType.Fettle || PlayerCombat.Instance == null) return;
        PlayerCombat.Instance.RecalculateHealth();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || axis != InputAxis.Cover) return;
        UiGearMenuController.Close();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}