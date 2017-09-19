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
    public class InventoryDisplay : MonoBehaviour
    {
        private Transform _inventoryContent;
        private Direction _inventoryDirection;
        private Inventory _inventory;
        private readonly List<InventoryItemUi> _inventoryItems = new List<InventoryItemUi>();
        private TextMeshProUGUI _titleText, _capacityText;
        private InventoryDisplay _moveToInventory;

        public void Awake()
        {
            _titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Inventory Title");
            _capacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Carrying Capacity");
            _inventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }

        public void SetInventory(Inventory inventory, Direction inventoryDirection, InventoryDisplay moveToInventory)
        {
            _moveToInventory = moveToInventory;
            _titleText.text = inventory.Name();
            _inventoryDirection = inventoryDirection;
            _inventoryItems.ForEach(i => i.DestroyItem());
            _inventoryItems.Clear();
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
            InventoryItemUi foundItem = _inventoryItems.FirstOrDefault(i =>
            {
                if (i.GetInventoryItem() is InventoryResource)
                {
                    return i.GetInventoryItem().Name() == inventoryItem.Name();
                }
                return i.GetInventoryItem() == inventoryItem;
            });
            if (foundItem == null) return;
            foundItem.Update();
            if (inventoryItem.Quantity() != 0 && _inventory.ContainsItem(inventoryItem)) return;
            foundItem.DestroyItem();
            _inventoryItems.Remove(foundItem);
            UpdateNavigation();
        }

        private void PopulateInventoryContents()
        {
            _inventory.Contents().ForEach(AddItem);
        }

        private void UpdateNavigation()
        {
            for (int i = 0; i < _inventoryItems.Count; ++i)
            {
                if (i <= 0) continue;
                GameObject from = _inventoryItems[i].GetGameObject();
                GameObject to = _inventoryItems[i - 1].GetGameObject();
                if (from == null || to == null) continue;
                Helper.SetReciprocalNavigation(@from, to);
                if (i == _inventoryItems.Count - 1)
                {
                    Helper.SetReciprocalNavigation(@from, InventoryTransferManager.CloseButton());
                }
            }
        }

        private bool IsItemDisplayed(BasicInventoryItem inventoryItem)
        {
            return _inventoryItems.Any(itemUi => itemUi.GetInventoryItem() == inventoryItem);
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
            _inventoryItems.Add(itemUi);
        }

        private void AddItem(BasicInventoryItem inventoryItem)
        {
            if (!IsItemDisplayed(inventoryItem))
            {
                CreateNewItem(inventoryItem);
            }
            UpdateItemUi(inventoryItem);
        }
    }
}