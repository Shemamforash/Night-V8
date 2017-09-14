using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryTransferManager : MonoBehaviour
    {
        private GameObject _inventory1, _inventory2;
        private readonly Vector2 _singleInventoryAnchorsMin = new Vector2(0.2f, 0.1f);
        private readonly Vector2 _singleInventoryAnchorsMax = new Vector2(0.8f, 0.9f);

        private readonly Vector2 _dualInventoryAnchorsMinLeft = new Vector2(0.1f, 0.1f);
        private readonly Vector2 _dualInventoryAnchorsMaxLeft = new Vector2(0.5f, 0.9f);
        
        private readonly Vector2 _dualInventoryAnchorsMinRight = new Vector2(0.5f, 0.1f);
        private readonly Vector2 _dualInventoryAnchorsMaxRight = new Vector2(0.9f, 0.9f);

        private readonly int _dualInventorySeparationOffset = 20;

        public InventoryDisplay InventoryLeft, InventoryRight;
        
        public void ShowSingleInventory(Inventory inventory)
        {
            InventoryLeft.gameObject.SetActive(true);
            InventoryRight.gameObject.SetActive(false);
            SetAnchors(InventoryLeft, _singleInventoryAnchorsMin, _singleInventoryAnchorsMax);
            InventoryLeft.SetInventory(inventory);
        }

        private void SetAnchors(InventoryDisplay inventoryDisplay, Vector2 minAnchors, Vector2 maxAnchors)
        {
            RectTransform inventoryTransform = inventoryDisplay.GetComponent<RectTransform>();
            inventoryTransform.anchorMin = minAnchors;
            inventoryTransform.anchorMax = maxAnchors;
        }

        public void ShowDualInventories(Inventory left, Inventory right)
        {
            InventoryLeft.gameObject.SetActive(true);
            InventoryRight.gameObject.SetActive(true);
            SetAnchors(InventoryLeft, _dualInventoryAnchorsMinLeft, _dualInventoryAnchorsMaxLeft);
            SetAnchors(InventoryRight, _dualInventoryAnchorsMinRight, _dualInventoryAnchorsMaxRight);
            InventoryLeft.SetInventory(left);
            InventoryRight.SetInventory(right);
        }
    }
}