using System;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryTransferManager : Menu
    {
        private GameObject _inventory1, _inventory2;
        private readonly Vector2 _singleInventoryAnchorsMin = new Vector2(0, 0.1f);
        private readonly Vector2 _singleInventoryAnchorsMax = new Vector2(1, 1);

        private readonly Vector2 _dualInventoryAnchorsMinLeft = new Vector2(0.1f, 0.2f);
        private readonly Vector2 _dualInventoryAnchorsMaxLeft = new Vector2(0.5f, 0.9f);

        private readonly Vector2 _dualInventoryAnchorsMinRight = new Vector2(0.5f, 0.2f);
        private readonly Vector2 _dualInventoryAnchorsMaxRight = new Vector2(0.9f, 0.9f);

        private readonly int _dualInventorySeparationOffset = 20;

        public InventoryDisplay InventoryLeft, InventoryRight;

        private Action _closeAction;
        private static Button _closeButton;

        private static InventoryTransferManager _instance;

        protected void Awake()
        {
            _instance = this;
            _closeButton = Helper.FindChildWithName<Button>(gameObject, "Confirm");
            _closeButton.onClick.AddListener(Close);
        }

        public static InventoryTransferManager Instance()
        {
            return _instance ?? FindObjectOfType<InventoryTransferManager>();
        }

        public static GameObject CloseButton()
        {
            return _closeButton.gameObject;
        }

        private void Close()
        {
            MenuStateMachine.States.NavigateToState("Game Menu");
            _closeAction?.Invoke();
            _closeAction = null;
        }

        public void ShowDualInventories(Inventory left, Inventory right, Action closeAction)
        {
            _closeAction = closeAction;
            ShowDualInventories(left, right);
        }

        public void ShowSingleInventory(Inventory inventory)
        {
            MenuStateMachine.States.NavigateToState("Inventory Menu");
            InventoryLeft.gameObject.SetActive(true);
            InventoryRight.gameObject.SetActive(false);
            SetAnchors(InventoryLeft, _singleInventoryAnchorsMin, _singleInventoryAnchorsMax);
            InventoryLeft.SetInventory(inventory, Direction.None, null);
        }

        private void SetAnchors<T>(T uiObject, Vector2 minAnchors, Vector2 maxAnchors) where T : MonoBehaviour
        {
            RectTransform inventoryTransform = uiObject.GetComponent<RectTransform>();
            inventoryTransform.anchorMin = minAnchors;
            inventoryTransform.anchorMax = maxAnchors;
        }

        public void ShowDualInventories(Inventory left, Inventory right)
        {
            MenuStateMachine.States.NavigateToState("Inventory Menu");
            InventoryLeft.gameObject.SetActive(true);
            InventoryRight.gameObject.SetActive(true);
            SetAnchors(InventoryLeft, _dualInventoryAnchorsMinLeft, _dualInventoryAnchorsMaxLeft);
            SetAnchors(InventoryRight, _dualInventoryAnchorsMinRight, _dualInventoryAnchorsMaxRight);
            InventoryLeft.SetInventory(left, Direction.Left, InventoryRight);
            InventoryRight.SetInventory(right, Direction.Right, InventoryLeft);
        }
    }
}