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
    private const int centre = 3;
    public const float MaxShowInventoryDistance = 0.5f;
    private static UiAreaInventoryController _instance;
    private readonly List<ItemUi> _gearUis = new List<ItemUi>();
    private Inventory _inventory;

    private int _selectedItem;

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        switch (axis)
        {
            case InputAxis.Vertical:
                if (direction < 0)
                {
                    TrySelectItemBelow();
                }
                else
                {
                    TrySelectItemAbove();
                }

                break;
            case InputAxis.Fire:
                UseItem();
                break;
            case InputAxis.Inventory:
                MenuStateMachine.ShowMenu("HUD");
                break;
        }
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        GameObject listObject = Helper.FindChildWithName(gameObject, "Player");
        for (int i = 0; i < 7; ++i)
        {
            GameObject uiObject = Helper.FindChildWithName(listObject, "Item " + i);
            ItemUi gearUi = new ItemUi(uiObject, Math.Abs(i - centre));
            _gearUis.Add(gearUi);
            gearUi.SetItem(null);
        }
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public static UiAreaInventoryController Instance() => _instance;

    public override void Enter()
    {
        base.Enter();
        InputHandler.SetCurrentListener(this);
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
        _inventory = CharacterManager.SelectedCharacter.Inventory();
        _selectedItem = 0;
        SelectItem();
        MenuStateMachine.ShowMenu("Inventory");
    }

    private void TrySelectItemBelow()
    {
        if (_selectedItem == _inventory.Contents().Count - 1) return;
        ++_selectedItem;
        SelectItem();
    }

    private void TrySelectItemAbove()
    {
        if (_selectedItem == 0) return;
        --_selectedItem;
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

    private void UseItem()
    {
        Consumable item = _inventory.Contents()[_selectedItem] as Consumable;
        if (item == null) return;
        item.Consume(PlayerCombat.Instance.Player);
        SelectItem();
    }

    public void TakeItem()
    {
        NearestContainer()?.Take();
    }

    private class ItemUi
    {
        private readonly Color _activeColour;
        private readonly EnhancedText _nameText;
        private readonly EnhancedText _quantityText;
        private readonly EnhancedText _weightText;

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
}