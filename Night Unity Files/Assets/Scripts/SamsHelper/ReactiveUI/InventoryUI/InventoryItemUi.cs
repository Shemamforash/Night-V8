using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryItemUi
    {
        private readonly GameObject _gameObject;
        private readonly BasicInventoryContents _inventoryItem;

        public InventoryItemUi(GameObject gameObject, BasicInventoryContents inventoryItem)
        {
            _gameObject = gameObject;
            _inventoryItem = inventoryItem;
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public BasicInventoryContents InventoryItem()
        {
            return _inventoryItem;
        }

        public void DestroyItem()
        {
            GameObject.Destroy(_gameObject);
        }
    }
}