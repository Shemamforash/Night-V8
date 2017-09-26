﻿using System;
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
    //this class should ONLY before for items that extend from InventoryItem
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
            SetItems(inventory.Contents());
        }

        private void UpdateInventoryWeight()
        {
            _capacityText.text = Helper.Round(_inventory.Weight, 1) + " kg";
        }

        public override BaseInventoryUi AddItem(MyGameObject item)
        {
            if (!(item is InventoryItem))
            {
                throw new Exceptions.InvalidInventoryItemException(item, "InventoryItem");
            }
            if (((InventoryItem) item).Quantity() == 0) return null;
            InventoryItemUi inventoryItemUi = (InventoryItemUi) base.AddItem(item);
            if (inventoryItemUi != null) //item is already added
            {
                inventoryItemUi.SetDirection(_inventoryDirection);
                inventoryItemUi.GetNavigationButton().GetComponent<EnhancedButton>().AddOnClick(() => GetMoveAction(item, 1)());
                inventoryItemUi.GetNavigationButton().GetComponent<EnhancedButton>().AddOnHold(GetMoveAction(item, 5), 0.5f);
            }
            return inventoryItemUi;
        }
        
        private Action GetMoveAction(MyGameObject inventoryItem, int quantity)
        {
            return () =>
            {
                MyGameObject transferredItem = _inventory.Move(inventoryItem, _moveToInventory._inventory, quantity);
                if (transferredItem == null) return;
                _moveToInventory.AddItem(transferredItem);
                UpdateItem(inventoryItem);
            };
        }

        protected override BaseInventoryUi UpdateItem(MyGameObject inventoryItem)
        {
            UpdateInventoryWeight();
            BaseInventoryUi found = base.UpdateItem(inventoryItem);
            InventoryItem foundItem = found.GetLinkedObject() as InventoryItem;
            if (foundItem != null && foundItem.Quantity() == 0 && !_inventory.ContainsItem(inventoryItem))
            {
                Remove(found);
                found.Destroy();
            }
            return null;
        }

        public override BaseInventoryUi RefreshNavigation()
        {
            BaseInventoryUi last = base.RefreshNavigation();
            if (last != null)
            {
                Helper.SetReciprocalNavigation(last.GetNavigationButton(), InventoryTransferManager.CloseButton());
            }
            return last;
        }
    }
}