using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiAreaInventoryController : Menu, IInputListener
{
    private static UiAreaInventoryController _instance;
    private readonly AreaInventory _playerInventory = new AreaInventory();
    private readonly AreaInventory _worldInventory = new AreaInventory();
    private const int centre = 3;
    private AreaInventory _selectedInventory;
    private bool _showPlayerInventoryOnly;
    public const float MaxShowInventoryDistance = 0.5f;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        _playerInventory.Initialise(Helper.FindChildWithName(gameObject, "Player"), _worldInventory);
        _worldInventory.Initialise(Helper.FindChildWithName(gameObject, "World"), _playerInventory);
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public override void Enter()
    {
        base.Enter();
        _selectedInventory = _playerInventory;
        if (_showPlayerInventoryOnly)
            _selectedInventory.SetActiveAlone();
        else
            _selectedInventory.SetActive();
        InputHandler.SetCurrentListener(this);
    }

    public override void Exit()
    {
        base.Exit();
        _showPlayerInventoryOnly = false;
    }

    private ContainerController NearestContainer()
    {
        ContainerController nearestContainer = null;
        float nearestContainerDistance = MaxShowInventoryDistance;
        ContainerController.Containers.ForEach(c =>
        {
            float distance = Vector2.Distance(c.transform.position, PlayerCombat.Instance.transform.position);
            if (distance > nearestContainerDistance) return;
            nearestContainerDistance = distance;
            nearestContainer = c.ContainerController;
        });
        return nearestContainer;
    }

    public void OpenInventory()
    {
        ContainerController nearestContainer = NearestContainer();

        if (nearestContainer != null)
        {
            _showPlayerInventoryOnly = false;
            _instance._playerInventory.SetInventory(CharacterManager.SelectedCharacter.Inventory());
            _instance._worldInventory.SetInventory(nearestContainer.Inventory());
            MenuStateMachine.ShowMenu("Inventory");
        }
        else
        {
            _instance.ShowPlayerInventory();
        }
    }

    private void ShowPlayerInventory()
    {
        _playerInventory.SetInventory(CharacterManager.SelectedCharacter.Inventory());
        _showPlayerInventoryOnly = true;
        MenuStateMachine.ShowMenu("Inventory");
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
                    _selectedInventory.SetActive();
                }
                else if (direction < 0 && _selectedInventory == _playerInventory)
                {
                    _selectedInventory = _worldInventory;
                    _selectedInventory.SetActive();
                }

                break;
            case InputAxis.Fire:
                PressItem();
                break;
            case InputAxis.Inventory:
                MenuStateMachine.ShowMenu("HUD");
                break;
        }
    }

    private void PressItem()
    {
        if (_showPlayerInventoryOnly)
        {
            _selectedInventory.UseItem();
        }
        else
        {
            AreaInventory otherInventory = _selectedInventory == _playerInventory ? _worldInventory : _playerInventory;
            _selectedInventory.TransferItem(otherInventory);
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
        private Inventory _inventory;
        private readonly List<ItemUi> _gearUis = new List<ItemUi>();
        private CanvasGroup _canvasGroup;
        private AreaInventory _otherInventory;

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

        public void SetActive()
        {
            _canvasGroup.alpha = 1f;
            _otherInventory._canvasGroup.alpha = 0.4f;
        }

        public void SetActiveAlone()
        {
            _canvasGroup.alpha = 1f;
            _otherInventory._canvasGroup.alpha = 0f;
        }

        public void SetInventory(Inventory inventory)
        {
            if (inventory == null)
            {
                _canvasGroup.alpha = 0f;
                return;
            }

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

                SetWeightText("Remove me");
                SetQuantityText("x" + inventoryItem.Quantity());
                SetNameText(inventoryItem.Name);
            }
        }

        public void Initialise(GameObject listObject, AreaInventory other)
        {
            _otherInventory = other;
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

        public void UseItem()
        {
            Consumable item = _inventory.Contents()[_selectedItem] as Consumable;
            if (item == null) return;
            item.Consume(PlayerCombat.Instance.Player);
            SelectItem();
        }
    }

    public static UiAreaInventoryController Instance()
    {
        return _instance;
    }
}