using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiConsumableController : Menu, IInputListener
{
    private static UiConsumableController _instance;
    private static int centre = 4;
    private int _selectedConsumable;
    private readonly List<ConsumableUi> _consumeableUis = new List<ConsumableUi>();
    private EnhancedButton _closeButton;
    private EnhancedButton _consumableButton;
    private bool _focussed;
    private List<Consumable> _consumables;
    private UIConditionController _thirstController, _hungerController;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        Initialise();
    }

    public static void ShowMenu()
    {
        MenuStateMachine.ShowMenu("Consumable Menu");
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

    private void UpdateCondition()
    {
        _hungerController.UpdateHunger(CharacterManager.SelectedCharacter);
        _thirstController.UpdateThirst(CharacterManager.SelectedCharacter);
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
                if (_focussed)
                {
                    _consumables[_selectedConsumable].Consume(CharacterManager.SelectedCharacter);
                    SelectItem();
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

    private void SelectItem()
    {
        UpdateCondition();
        _consumables = WorldState.HomeInventory().Consumables();

        if (_selectedConsumable >= _consumables.Count) _selectedConsumable = _consumables.Count - 1;
        for (int i = 0; i < _consumeableUis.Count; ++i)
        {
            int offset = i - centre;
            int targetConsumableIndex = _selectedConsumable + offset;
            Consumable consumable = null;
            if (targetConsumableIndex >= 0 && targetConsumableIndex < _consumables.Count) consumable = _consumables[targetConsumableIndex];
            _consumeableUis[i].SetConsumable(consumable);
        }

        if (_consumables.Count == 0)
        {
            _focussed = false;
            _closeButton.Select();
            return;
        }

        _consumableButton.Select();
        _focussed = true;
    }

    private void Initialise()
    {
        GameObject listObject = Helper.FindChildWithName(gameObject, "Consumable List");
        int count = 0;
        centre = listObject.transform.childCount / 2;
        foreach (Transform child in listObject.transform)
        {
            ConsumableUi consumableUi = new ConsumableUi(child.gameObject, Math.Abs(count - centre));
            if (count == centre)
            {
                _consumableButton = child.transform.GetComponent<EnhancedButton>();
            }

            _consumeableUis.Add(consumableUi);
            consumableUi.SetConsumable(null);
            ++count;
        }

        _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Close");
        _closeButton.AddOnClick(MenuStateMachine.ReturnToDefault);

        _thirstController = Helper.FindChildWithName<UIConditionController>(gameObject, "Thirst");
        _hungerController = Helper.FindChildWithName<UIConditionController>(gameObject, "Hunger");
    }

    private void TrySelectItemBelow()
    {
        if (!_focussed)
        {
            SelectAtPosition(0);
            return;
        }
        if (_selectedConsumable == _consumables.Count - 1)
        {
            _focussed = false;
            _closeButton.Select();
            return;
        }
        ++_selectedConsumable;
        SelectItem();
    }

    private void TrySelectItemAbove()
    {
        if (!_focussed)
        {
            SelectAtPosition(_consumables.Count - 1);
            return;
        }
        if (_selectedConsumable == 0)
        {
            _focussed = false;
            _closeButton.Select();
            return;
        }

        --_selectedConsumable;
        SelectItem();
    }

    private void SelectAtPosition(int position)
    {
        if (_consumables.Count == 0)
        {
            _closeButton.Select();
            return;
        }

        _selectedConsumable = position;
        SelectItem();
    }

    private class ConsumableUi
    {
        private readonly Color _activeColour;
        private readonly EnhancedText _nameText;
        private readonly EnhancedText _effectText;

        public ConsumableUi(GameObject uiObject, int offset)
        {
            _nameText = Helper.FindChildWithName<EnhancedText>(uiObject, "Name");
            _effectText = Helper.FindChildWithName<EnhancedText>(uiObject, "Effect");
            _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
        }

        private void SetColour(Color c)
        {
            _effectText.SetColor(c);
            _nameText.SetColor(c);
        }

        private void SetName(string text, int quantity)
        {
            _nameText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
            if (quantity > 1) text += " x" + quantity;
            _nameText.Text(text);
        }

        private void SetEffectText(string type)
        {
            _effectText.SetColor(type == "" ? UiAppearanceController.InvisibleColour : _activeColour);
            _effectText.Text(type);
        }

        public void SetConsumable(Consumable consumable)
        {
            if (consumable == null)
            {
                SetColour(UiAppearanceController.InvisibleColour);
                return;
            }

            SetName(consumable.Name, consumable.Quantity());
            SetEffectText(consumable.Effect());
        }
    }
}