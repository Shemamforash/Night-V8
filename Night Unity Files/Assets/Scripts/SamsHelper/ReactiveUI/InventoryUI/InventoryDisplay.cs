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
using SamsHelper.ReactiveUI.Elements;
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
            List<MyGameObject> inventoryContents = new List<MyGameObject>();
            inventory.Contents().ForEach(item =>
            {
                InventoryResource resource = item as InventoryResource;
                if (resource == null || resource.Quantity() != 0)
                {
                    inventoryContents.Add(resource);
                }
            });
            SetItems(inventoryContents);
        }

        private void UpdateInventoryWeight()
        {
            _capacityText.text = Helper.Round(_inventory.Weight, 1) + " kg";
        }

        public override BaseInventoryUi Add(BaseInventoryUi item)
        {
            InventoryItemUi inventoryItemUi = (InventoryItemUi) base.Add(item);
            inventoryItemUi.SetDirection(_inventoryDirection);
            inventoryItemUi.GetNavigationButton().GetComponent<EnhancedButton>().AddOnClick(() => GetMoveAction(item.GetLinkedObject(), 1)());
            inventoryItemUi.GetNavigationButton().GetComponent<EnhancedButton>().AddOnHold(GetMoveAction(item.GetLinkedObject(), 5), 0.5f);
            return inventoryItemUi;
        }

        private Action GetMoveAction(MyGameObject inventoryItem, int quantity)
        {
            return () =>
            {
                MyGameObject transferredItem = _inventory.Move(inventoryItem, _moveToInventory._inventory, quantity);
                if (transferredItem == null) return;
                _moveToInventory.Add(transferredItem);
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
                Remove(found);
                found.Destroy();
            }
            return null;
        }

        protected override List<BaseInventoryUi> GetNavigatableItems(List<BaseInventoryUi> items)
        {
            List<BaseInventoryUi> navigatableItems = new List<BaseInventoryUi>();
            items.ForEach(item =>
            {
                InventoryItemUi itemUi = (InventoryItemUi) item;
                if (itemUi.GetRightButton().activeInHierarchy || itemUi.GetLeftButton().activeInHierarchy)
                {
                    navigatableItems.Add(itemUi);
                }
            });
            return items;
            return navigatableItems;
        }

        public override BaseInventoryUi RefreshNavigation()
        {
            BaseInventoryUi last = base.RefreshNavigation();
            Helper.SetReciprocalNavigation(last.GetNavigationButton(), InventoryTransferManager.CloseButton());
            return last;
        }
    }
}