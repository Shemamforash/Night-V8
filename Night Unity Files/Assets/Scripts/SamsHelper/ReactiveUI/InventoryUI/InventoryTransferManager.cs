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
//        private readonly Vector2 _singleInventoryAnchorsMin = new Vector2(0, 0.1f);
//        private readonly Vector2 _singleInventoryAnchorsMax = new Vector2(1, 1);
//
//        private readonly Vector2 _dualInventoryAnchorsMinLeft = new Vector2(0.1f, 0.2f);
//        private readonly Vector2 _dualInventoryAnchorsMaxLeft = new Vector2(0.5f, 0.9f);
//
//        private readonly Vector2 _dualInventoryAnchorsMinRight = new Vector2(0.5f, 0.2f);
//        private readonly Vector2 _dualInventoryAnchorsMaxRight = new Vector2(0.9f, 0.9f);
//
//        private readonly int _dualInventorySeparationOffset = 20;

        public InventoryDisplay InventoryLeft, InventoryRight;

        private Action _closeAction;
        private static Button _closeButton;

        private static InventoryTransferManager _instance;

        protected void Awake()
        {
            _instance = this;
            _closeButton = Helper.FindChildWithName<Button>(gameObject, "Confirm");
            _closeButton.onClick.AddListener(Close);
            InventoryLeft.AddOnContentChange(RefreshNavigation);
            InventoryRight.AddOnContentChange(RefreshNavigation);
        }

        public static InventoryTransferManager Instance()
        {
            return _instance ?? FindObjectOfType<InventoryTransferManager>();
        }

        public static Button CloseButton()
        {
            return _closeButton;
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

        private void SetNavigation(InventoryDisplay inventoryRight, Direction direction, ViewParent inventoryLeftItem)
        {
            EnhancedButton target = inventoryLeftItem.GetNavigationButton();
            inventoryRight.Items.ForEach(i =>
            {
                EnhancedButton origin = i.GetNavigationButton();
                if (direction == Direction.Left)
                {
                    origin.SetLeftNavigation(target);
                    return;
                }
                origin.SetRightNavigation(target);
            });
        }

        private void ShowInventories(Inventory left, Inventory right)
        {
            MenuStateMachine.States.NavigateToState("Inventory Transfer Menu");
            InventoryLeft.gameObject.SetActive(true);
            InventoryRight.gameObject.SetActive(true);
            InventoryLeft.SetInventory(left, InventoryRight);
            InventoryRight.SetInventory(right, InventoryLeft);
        }

        private void RefreshNavigation()
        {
            if (InventoryRight.Items.Count == 0 || InventoryLeft.Items.Count == 0) return;
            SetNavigation(InventoryLeft, Direction.Right, InventoryRight.Items[0]);
            SetNavigation(InventoryRight, Direction.Left, InventoryLeft.Items[0]);
        }
    }
}