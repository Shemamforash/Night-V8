using System;
using System.Collections.Generic;
using System.Linq;
using Facilitating.UI.Elements;
using Facilitating.UI.Inventory;
using Game.Characters;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryDisplay : MenuList
    {
        private Direction _inventoryDirection;
        private Inventory _inventory;
        private TextMeshProUGUI _titleText, _capacityText;
        private InventoryDisplay _moveToInventory;

        public override void Awake()
        {
            base.Awake();
            _titleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Inventory Title");
            _capacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Carrying Capacity");
        }

        public void SetInventory(Inventory inventory, Direction inventoryDirection, InventoryDisplay moveToInventory)
        {
            _moveToInventory = moveToInventory;
            _titleText.text = inventory.Name;
            _inventoryDirection = inventoryDirection;
            _inventory = inventory;
            RectTransform rect = InventoryContent.GetComponent<RectTransform>();
            if (inventoryDirection == Direction.None)
            {
                rect.offsetMin = new Vector2(200, 0);
                rect.offsetMax = new Vector2(-200, 0);
            }
            else
            {
                rect.offsetMin = new Vector2(0, 0);
                rect.offsetMax = new Vector2(0, 0);
            }
            SetItems(inventory.Contents());
        }

        protected override BaseInventoryUi RestrictedContentCheck(MyGameObject myGameObject)
        {
            BasicInventoryItem o = myGameObject as BasicInventoryItem;
            InventoryItemUi itemUi = null;
            if (myGameObject is InventoryResource)
            {
                itemUi = new InventoryItemUi(o, InventoryContent, _inventoryDirection);
            }
            if (myGameObject is EquippableItem)
            {
                itemUi = new GearInventoryUi((EquippableItem)o, InventoryContent, _inventoryDirection == Direction.None, _inventoryDirection);
            }
            if (itemUi != null && _moveToInventory != null)
            {
                switch (_inventoryDirection)
                {
                    case Direction.Left:
                        itemUi.OnRightButtonPress(GetMoveAction(myGameObject, 1));
                        itemUi.OnRightButtonHold(GetMoveAction(myGameObject, 5), 0.5f);
                        break;
                    case Direction.Right:
                        itemUi.OnLeftButtonPress(GetMoveAction(myGameObject, 1));
                        itemUi.OnRightButtonHold(GetMoveAction(myGameObject, 5), 0.5f);
                        break;
                }
            }
            return itemUi;
        }

        private void UpdateInventoryWeight()
        {
            _capacityText.text = Helper.Round(_inventory.Weight, 1) + " kg";
        }

        private Action GetMoveAction(MyGameObject inventoryItem, int quantity)
        {
            return () =>
            {
                MyGameObject transferredItem = _inventory.Move(inventoryItem, _moveToInventory._inventory, quantity);
                if (transferredItem == null) return;
                _moveToInventory.Add(RestrictedContentCheck(transferredItem));
                UpdateItem(inventoryItem);
            };
        }

        protected override BaseInventoryUi UpdateItem(MyGameObject inventoryItem)
        {
            UpdateInventoryWeight();
            BaseInventoryUi found = base.UpdateItem(inventoryItem);
            BasicInventoryItem foundItem = found.GetLinkedObject() as BasicInventoryItem;
            if (foundItem != null && foundItem.Quantity() == 0 && !_inventory.ContainsItem(inventoryItem))
            {
                found.Destroy();
                Remove(found);
            }
            return null;
        }

        protected override List<BaseInventoryUi> GetNavigatableItems(List<BaseInventoryUi> items)
        {
            List<BaseInventoryUi> navigatableItems = new List<BaseInventoryUi>();
            items.ForEach(item =>
            {
                InventoryItemUi itemUi = (InventoryItemUi)item;
                if (itemUi.GetRightButton().activeInHierarchy || itemUi.GetLeftButton().activeInHierarchy)
                {
                    navigatableItems.Add(itemUi);
                }
            });
            return items;
            return navigatableItems;
        }

        protected override BaseInventoryUi SetNavigation()
        {
            BaseInventoryUi last = base.SetNavigation();
            Helper.SetReciprocalNavigation(last.GetNavigationButton(), InventoryTransferManager.CloseButton());
            return last;
        }
    }
}