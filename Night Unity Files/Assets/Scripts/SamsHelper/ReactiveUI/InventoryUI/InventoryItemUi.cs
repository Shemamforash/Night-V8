using System;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryItemUi : SimpleItemUi
    {
        protected readonly BasicInventoryItem InventoryItem;

        public InventoryItemUi(BasicInventoryItem inventoryItem, Transform parent, Direction direction = Direction.None) : base(parent)
        {
            InventoryItem = inventoryItem;
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
                    SummaryText.GetComponent<LayoutElement>().minWidth = 300;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public BasicInventoryItem GetInventoryItem()
        {
            return InventoryItem;
        }

        public override void Update()
        {
            if (InventoryItem is InventoryResource)
            {
                SummaryText.text = "x" + InventoryItem.Quantity();
            }
            _nameText.text = InventoryItem.ExtendedName();
            _weightText.text = InventoryItem.TotalWeight() + "kg";
            _typeText.text = InventoryItem.GetItemType();
        }
    }
}