using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryDisplay : MonoBehaviour
    {
        public Transform InventoryContent;
        public GameObject ItemPrefab;
        private Inventory _inventory;
        private readonly List<InventoryItemUi> _inventoryItems = new List<InventoryItemUi>();
        private Button _closeButton;
        protected TextMeshProUGUI TitleText, CapacityText;

        public virtual void Awake()
        {
            _closeButton = Helper.FindChildWithName<Button>(gameObject, "Close");
            _closeButton.onClick.AddListener(Close);
            TitleText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Title");
            CapacityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Capacity");
        }

        protected virtual void Close()
        {
        }

        public void SetInventory(Inventory inventory)
        {
            _inventoryItems.ForEach(i => i.DestroyItem());
            _inventoryItems.Clear();
            _inventory = inventory;
            PopulateInventoryContents();
        }

        private void PopulateInventoryContents()
        {
            List<BasicInventoryContents> inventoryContents = _inventory.Contents();
            for (int i = 0; i < inventoryContents.Count; ++i)
            {
                BasicInventoryContents inventoryItem = inventoryContents[i];
                GameObject uiObject = AddItem(inventoryItem);
                if (i > 0)
                {
                    Helper.SetNavigation(uiObject, _inventoryItems[i - 1].GetGameObject(), Helper.NavigationDirections.Up);
                    Helper.SetNavigation(_inventoryItems[i - 1].GetGameObject(), uiObject, Helper.NavigationDirections.Down);
                }
                InventoryItemUi itemUi = new InventoryItemUi(uiObject, inventoryItem);
                _inventoryItems.Add(itemUi);
            }
            Helper.SetReciprocalNavigation(_inventoryItems[_inventoryItems.Count - 1].GetGameObject(), _closeButton.gameObject);
        }

        protected virtual GameObject AddItem(BasicInventoryContents inventoryItem)
        {
            return Helper.InstantiateUiObject(ItemPrefab, InventoryContent);
        }
    }
}