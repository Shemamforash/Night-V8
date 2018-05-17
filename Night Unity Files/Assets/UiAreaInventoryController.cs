using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiAreaInventoryController : Menu, IInputListener
{
    private static ContainerController _lastNearestContainer;
    private static UiAreaInventoryController _instance;
    private readonly AreaInventory _playerInventory = new AreaInventory();
    private readonly AreaInventory _worldInventory = new AreaInventory();
    private const int centre = 3;
    private AreaInventory _selectedInventory;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        _lastNearestContainer = null;
        _playerInventory.Initialise(Helper.FindChildWithName(gameObject, "Player"));
        _worldInventory.Initialise(Helper.FindChildWithName(gameObject, "World"));
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public override void Enter()
    {
        base.Enter();
        _selectedInventory = _playerInventory;
        _selectedInventory.SetActive(_worldInventory);
        InputHandler.SetCurrentListener(this);
    }

    public static void SetNearestContainer(ContainerController nearestContainer)
    {
        if (nearestContainer != null)
        {
            if (nearestContainer != _lastNearestContainer)
            {
                _instance._playerInventory.SetInventory(CharacterManager.SelectedCharacter.Inventory());
                _instance._worldInventory.SetInventory(nearestContainer.Inventory);
                MenuStateMachine.ShowMenu("Inventory");
            }
        }

        else if (_lastNearestContainer != null)
        {
            MenuStateMachine.ShowMenu("HUD");
        }

        _lastNearestContainer = nearestContainer;
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        switch (axis)
        {
            case InputAxis.Vertical:
                if (direction < 0)
                    _selectedInventory.TrySelectItemBelow();
                else
                    _selectedInventory.TrySelectItemAbove();
                break;
            case InputAxis.Horizontal:
                if (direction > 0 && _selectedInventory == _worldInventory)
                {
                    _selectedInventory = _playerInventory;
                    _selectedInventory.SetActive(_worldInventory);
                }
                else if (direction < 0 && _selectedInventory == _playerInventory)
                {
                    _selectedInventory = _worldInventory;
                    _selectedInventory.SetActive(_playerInventory);
                }

                break;
            case InputAxis.Fire:
                AreaInventory otherInventory = _selectedInventory == _playerInventory ? _worldInventory : _playerInventory;
                _selectedInventory.TransferItem(otherInventory);
                break;
            case InputAxis.Reload:
                SetNearestContainer(null);
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

    private class AreaInventory
    {
        private int _selectedItem;
        private DesolationInventory _inventory;
        private readonly List<ItemUi> _gearUis = new List<ItemUi>();
        private CanvasGroup _canvasGroup;

        public void TrySelectItemBelow()
        {
            if (_selectedItem == _inventory.Contents().Count - 1) return;
            ++_selectedItem;
            SelectItem();
        }

        public void TrySelectItemAbove()
        {
            if (_selectedItem == 0) return;
            --_selectedItem;
            SelectItem();
        }

        public void SetActive(AreaInventory inventory)
        {
            _canvasGroup.alpha = 1f;
            inventory._canvasGroup.alpha = 0.4f;
        }

        public void SetInventory(DesolationInventory inventory)
        {
            _inventory = inventory;
            _selectedItem = 0;
            SelectItem();
        }

        private void SelectItem()
        {
            List<InventoryItem> inventoryContents = _inventory.Contents();
            if (_selectedItem >= inventoryContents.Count) _selectedItem = inventoryContents.Count - 1;
            for (int i = 0; i < _gearUis.Count; ++i)
            {
                int offset = i - centre;
                int targetGear = _selectedItem + offset;
                InventoryItem gearItem = null;
                if (targetGear >= 0 && targetGear < inventoryContents.Count) gearItem = inventoryContents[targetGear];
                _gearUis[i].SetItem(gearItem);
            }
        }

        private class ItemUi
        {
            private readonly Color _activeColour;
            private readonly EnhancedText _weightText;
            private readonly EnhancedText _quantityText;
            private readonly EnhancedText _nameText;

            public ItemUi(GameObject uiObject, int offset)
            {
                _nameText = Helper.FindChildWithName<EnhancedText>(uiObject, "Name");
                _quantityText = Helper.FindChildWithName<EnhancedText>(uiObject, "Quantity");
                _weightText = Helper.FindChildWithName<EnhancedText>(uiObject, "Weight");
                _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
            }

            private void SetColour(Color c)
            {
                _nameText.SetColor(c);
                _quantityText.SetColor(c);
                _weightText.SetColor(c);
            }

            private void SetWeightText(string text)
            {
                _weightText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _weightText.Text(text);
            }

            private void SetQuantityText(string text)
            {
                _quantityText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _quantityText.Text(text);
            }

            private void SetNameText(string text)
            {
                _nameText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _nameText.Text(text);
            }

            public void SetItem(InventoryItem inventoryItem)
            {
                if (inventoryItem == null)
                {
                    SetColour(UiAppearanceController.InvisibleColour);
                    return;
                }

                SetWeightText(inventoryItem.Weight + "kg");
                SetQuantityText("x" + inventoryItem.Quantity());
                SetNameText(inventoryItem.Name);
            }
        }

        public void Initialise(GameObject listObject)
        {
            _canvasGroup = listObject.GetComponent<CanvasGroup>();
            for (int i = 0; i < 7; ++i)
            {
                GameObject uiObject = Helper.FindChildWithName(listObject, "Item " + i);
                ItemUi gearUi = new ItemUi(uiObject, Math.Abs(i - centre));
                _gearUis.Add(gearUi);
                gearUi.SetItem(null);
            }
        }

        public void TransferItem(AreaInventory otherInventory)
        {
            otherInventory._inventory.Move(_inventory.Contents()[_selectedItem], 1);
            SelectItem();
            otherInventory.SelectItem();
        }
    }
}