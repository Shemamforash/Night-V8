using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryItemUi
    {
        private readonly GameObject _gameObject;
        private readonly BasicInventoryItem _inventoryItem;
        private readonly TextMeshProUGUI _weightText, _quantityText;
        private readonly GameObject _moveButton;

        public InventoryItemUi(GameObject gameObject, BasicInventoryItem inventoryItem)
        {
            _gameObject = gameObject;
            _inventoryItem = inventoryItem;
            Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name").text = inventoryItem.Name();
            _weightText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Weight");
            _quantityText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Quantity");
            _moveButton = Helper.FindChildWithName(gameObject, "Move");
            UpdateWeightAndQuantity();
        }

        public GameObject MoveButton()
        {
            return _moveButton;
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public BasicInventoryItem InventoryItem()
        {
            return _inventoryItem;
        }

        public void DestroyItem()
        {
            GameObject.Destroy(_gameObject);
        }

        public void UpdateWeightAndQuantity()
        {
            float weight = _inventoryItem.Weight() * _inventoryItem.Quantity();
            Helper.Round(weight, 1);
            _weightText.text = weight + " <sprite name=\"Weight\">";
            int quantity = _inventoryItem.Quantity();
            _quantityText.text = quantity == 0 ? "" : quantity.ToString();
        }
    }
}