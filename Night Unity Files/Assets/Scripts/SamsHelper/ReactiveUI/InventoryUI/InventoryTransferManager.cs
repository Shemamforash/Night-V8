using System;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryTransferManager : Menu
    {
        private GameObject _inventory1, _inventory2;
        public InventoryDisplay InventoryLeft, InventoryRight;

        private Action _closeAction;
        private static EnhancedButton _closeButton;

        private static InventoryTransferManager _instance;

        protected void Awake()
        {
            _instance = this;
            _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Confirm");
            _closeButton.AddOnClick(Close);
            InventoryLeft.AddOnContentChange(RefreshNavigation);
            InventoryRight.AddOnContentChange(RefreshNavigation);
        }

        public static InventoryTransferManager Instance()
        {
            return _instance ?? FindObjectOfType<InventoryTransferManager>();
        }

        private void Close()
        {
            MenuStateMachine.States.NavigateToState("Game Menu");
            _closeAction?.Invoke();
            _closeAction = null;
        }

        public void ShowInventories(Inventory left, Inventory right, Action closeAction)
        {
            _closeAction = closeAction;
            ShowInventories(left, right);
        }

        private void SetNavigation(InventoryDisplay inventory, Direction direction, ViewParent targetItem)
        {
            EnhancedButton targetButton = targetItem.PrimaryButton;
            inventory.Items.ForEach(i =>
            {
                EnhancedButton origin = i.PrimaryButton;
                if (direction == Direction.Left)
                {
                    origin.SetLeftNavigation(targetButton);
                    return;
                }
                origin.SetRightNavigation(targetButton);
            });
        }

        private void ShowInventories(Inventory left, Inventory right)
        {
            MenuStateMachine.States.NavigateToState("Inventory Transfer Menu");
            InventoryLeft.gameObject.SetActive(true);
            InventoryRight.gameObject.SetActive(true);
            InventoryLeft.SetInventory(left, InventoryRight, _closeButton);
            InventoryRight.SetInventory(right, InventoryLeft, _closeButton);
        }

        private void RefreshNavigation()
        {
            if (InventoryRight.Items.Count == 0 || InventoryLeft.Items.Count == 0) return;
            SetNavigation(InventoryLeft, Direction.Right, InventoryRight.Items[0]);
            SetNavigation(InventoryRight, Direction.Left, InventoryLeft.Items[0]);
        }
    }
}