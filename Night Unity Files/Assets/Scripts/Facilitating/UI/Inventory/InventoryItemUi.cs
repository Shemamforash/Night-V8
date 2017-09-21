using System;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Inventory
{
    public class InventoryItemUi : BaseInventoryUi
    {
        private readonly BasicInventoryItem _item;
        protected TextMeshProUGUI TypeText, WeightText;
        protected TextMeshProUGUI SummaryText, ButtonText;

        public InventoryItemUi(BasicInventoryItem item, Transform parent, Direction direction = Direction.None) : base(item, parent)
        {
            _item = item;
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

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            TypeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Type");
            SummaryText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Summary");
            WeightText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Weight");
            ButtonText = Helper.FindChildWithName<TextMeshProUGUI>(ActionButton.gameObject, "Text");
        }

        public override void Update()
        {
            if (_item.Quantity() == 0)
            {
                Destroy();
            }
            else
            {
                if (_item is InventoryResource)
                {
                    SummaryText.text = "x" + _item.Quantity();
                }
                NameText.text = _item.ExtendedName();
                WeightText.text = _item.TotalWeight() + "kg";
                TypeText.text = _item.Type.ToString();
            }
        }
    }
}