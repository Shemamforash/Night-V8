using System;
using System.Collections.Generic;
using System.Linq;
using Facilitating.UI.Elements;
using Facilitating.UI.Inventory;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryDisplay : MenuList<InventoryItemUi>
    {
        private Direction _inventoryDirection;
        private Inventory _inventory;
        private TextMeshProUGUI _titleText, _capacityText;
        private InventoryDisplay _moveToInventory;
        private Func<BasicInventoryItem, bool> _showOnlyAction;

        public override void Awake()
        {
            base.Awake();
            _titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Inventory Title");
            _capacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Carrying Capacity");
        }

        public void SetInventory(Inventory inventory, Direction inventoryDirection, InventoryDisplay moveToInventory, Func<BasicInventoryItem, bool> showOnlyAction)
        {
            _moveToInventory = moveToInventory;
            _showOnlyAction = showOnlyAction;
            _titleText.text = inventory.Name();
            _inventoryDirection = inventoryDirection;
            _inventory = inventory;
            PopulateInventoryContents();
        }

        private void UpdateInventoryWeight()
        {
            _capacityText.text = Helper.Round(_inventory.GetInventoryWeight(), 1) + " kg";
        }

        private Action GetMoveAction(BasicInventoryItem inventoryItem, int quantity)
        {
            return () =>
            {
                BasicInventoryItem transferredItem = _inventory.Move(inventoryItem, _moveToInventory._inventory, quantity);
                if (transferredItem == null) return;
                _moveToInventory.AddItem(transferredItem);
                UpdateItemUi(inventoryItem);
            };
        }

        private void UpdateItemUi(BasicInventoryItem inventoryItem)
        {
            UpdateInventoryWeight();
            InventoryItemUi foundItem = _items.FirstOrDefault(i =>
            {
                if (i.GetInventoryItem() is InventoryResource)
                {
                    return i.GetInventoryItem().Name() == inventoryItem.Name();
                }
                return i.GetInventoryItem() == inventoryItem;
            });
            if (foundItem != null)
            {
                foundItem.Update();
                if (inventoryItem.Quantity() == 0 && !_inventory.ContainsItem(inventoryItem))
                {
                    foundItem.Destroy();
                    RemoveItem(foundItem);
                }
            }
            SetNavigation();
        }

        private void PopulateInventoryContents()
        {
            _inventory.Contents().ForEach(AddItem);
        }

        protected override InventoryItemUi SetNavigation()
        {
            InventoryItemUi last = base.SetNavigation();
            Helper.SetReciprocalNavigation(last.GetButton(), InventoryTransferManager.CloseButton());
            return last;
        }

        private bool IsItemDisplayed(BasicInventoryItem inventoryItem)
        {
            return _items.Any(itemUi => itemUi.GetInventoryItem() == inventoryItem);
        }

        private void CreateNewItem(BasicInventoryItem inventoryItem)
        {
            InventoryItemUi itemUi;
            if (inventoryItem is EquippableItem)
            {
                if (_inventoryDirection == Direction.None)
                {
                    itemUi = new GearInventoryUi(inventoryItem, _inventoryContent, true);
                }
                else
                {
                    itemUi = new GearInventoryUi(inventoryItem, _inventoryContent, false);
                }
            }
            else
            {
                itemUi = new InventoryItemUi(inventoryItem, _inventoryContent, _inventoryDirection);
            }
            if (_moveToInventory != null)
            {
                itemUi.OnActionPress(GetMoveAction(inventoryItem, 1));
                itemUi.OnActionHold(GetMoveAction(inventoryItem, 5), 0.5f);
            }
            _items.Add(itemUi);
        }

        private void AddItem(BasicInventoryItem inventoryItem)
        {
            if (_showOnlyAction != null && !_showOnlyAction(inventoryItem)) return;
            if (!IsItemDisplayed(inventoryItem))
            {
                CreateNewItem(inventoryItem);
            }
            UpdateItemUi(inventoryItem);
        }
    }
}