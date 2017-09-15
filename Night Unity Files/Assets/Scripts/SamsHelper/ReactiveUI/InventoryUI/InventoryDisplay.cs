using System.Collections.Generic;
using System.Linq;
using Facilitating.UI.Elements;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryDisplay : MonoBehaviour
    {
        private Transform _inventoryContent;
        private GameObject _itemPrefab;
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

        public void SetInventory(Inventory inventory, GameObject itemPrefab, InventoryDisplay moveToInventory)
        {
            _moveToInventory = moveToInventory;
            _titleText.text = inventory.Name();
            _itemPrefab = itemPrefab;
            _inventoryItems.ForEach(i => i.DestroyItem());
            _inventoryItems.Clear();
            _inventory = inventory;
            PopulateInventoryContents();
        }

        private void UpdateInventoryWeight()
        {
            _capacityText.text = _inventory.GetInventoryWeight() + " W";
        }

        private UnityAction GetMoveAction(BasicInventoryItem inventoryItem, int quantity)
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
            InventoryItemUi foundItem = _inventoryItems.FirstOrDefault(i => i.InventoryItem().Name() == inventoryItem.Name());
            if (foundItem == null) return;
            foundItem.UpdateWeightAndQuantity();
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
                GameObject from = _inventoryItems[i].MoveButton();
                GameObject to = _inventoryItems[i - 1].MoveButton();
                Helper.SetReciprocalNavigation(from, to);
                if (i == _inventoryItems.Count - 1)
                {
                    Helper.SetReciprocalNavigation(from, InventoryTransferManager.CloseButton());
                }
            }
        }

        private bool IsItemDisplayed(BasicInventoryItem inventoryItem)
        {
            return _inventoryItems.Any(itemUi => itemUi.InventoryItem() == inventoryItem);
        }

        private void CreateNewItem(BasicInventoryItem inventoryItem)
        {
            GameObject uiObject = Helper.InstantiateUiObject(_itemPrefab, _inventoryContent);
            InventoryItemUi itemUi = new InventoryItemUi(uiObject, inventoryItem);
            if (_moveToInventory != null)
            {
                EnhancedButton button = itemUi.MoveButton().GetComponent<EnhancedButton>();
                button.AddOnClick(GetMoveAction(inventoryItem, 1));
                button.AddOnHold(() => GetMoveAction(inventoryItem, 5), 0.5f);
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