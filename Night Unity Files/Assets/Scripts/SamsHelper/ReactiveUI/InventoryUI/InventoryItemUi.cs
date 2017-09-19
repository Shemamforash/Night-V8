using System;
using Facilitating.UI.Elements;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryItemUi
    {
        private readonly GameObject _gameObject;
        protected readonly BasicInventoryItem InventoryItem;
        protected readonly EnhancedButton ActionButton;
        private readonly TextMeshProUGUI _typeText, _nameText, _weightText;
        protected readonly TextMeshProUGUI SummaryText, ButtonText;

        public InventoryItemUi(BasicInventoryItem inventoryItem, Transform parent, Direction direction = Direction.None)
        {
            InventoryItem = inventoryItem;
            _gameObject = Helper.InstantiateUiObject("Prefabs/Menu Button", parent);
            ActionButton = Helper.FindChildWithName<EnhancedButton>(_gameObject, "Action Button");
            _typeText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Type");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Name");
            SummaryText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Summary");
            _weightText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Weight");
            ButtonText = Helper.FindChildWithName<TextMeshProUGUI>(ActionButton.gameObject, "Text");
            SetActionButtonText(direction);
        }

        private void SetActionButtonText(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    ButtonText.text = "^^";
                    break;
                case Direction.Down:
                    ButtonText.text = "\\/\\/";
                    break;
                case Direction.Left:
                    ButtonText.text = "<<";
                    break;
                case Direction.Right:
                    ButtonText.text = ">>";
                    break;
                case Direction.None:
                    ActionButton.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public BasicInventoryItem GetInventoryItem()
        {
            return InventoryItem;
        }

        public void DestroyItem()
        {
            GameObject.Destroy(_gameObject);
        }

        public virtual void Update()
        {
            if (InventoryItem is InventoryResource)
            {
                SummaryText.text = "x" + InventoryItem.Quantity();
            }
            _nameText.text = InventoryItem.ExtendedName();
            _weightText.text = InventoryItem.TotalWeight() + "kg";
            _typeText.text = InventoryItem.GetItemType().ToString();
        }

        public void OnActionPress(Action a)
        {
            ActionButton.AddOnClick(() => a());
        }

        public void OnActionHold(Action a, float duration)
        {
            ActionButton.AddOnHold(a, duration);
        }
    }
}