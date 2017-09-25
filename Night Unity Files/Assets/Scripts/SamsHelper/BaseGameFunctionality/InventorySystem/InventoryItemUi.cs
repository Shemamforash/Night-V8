using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Facilitating.UI.Elements;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI.Inventory
{
    public class InventoryItemUi : BaseInventoryUi
    {
        protected EnhancedButton LeftActionButton, RightActionButton;
        protected TextMeshProUGUI LeftButtonText, RightButtonText;
        protected TextMeshProUGUI TypeText, WeightText;
        protected TextMeshProUGUI SummaryText;
        protected GameObject Bookends;
        protected Direction Direction;

        public InventoryItemUi(MyGameObject item, Transform parent, Direction direction = Direction.None) : base(item, parent, "Prefabs/Inventory/FlexibleItem")
        {
            Direction = direction;
            GameObject.GetComponent<LayoutElement>().preferredHeight = 40;
        }

        public void SetDirection(Direction direction)
        {
            Direction = direction;
            switch (Direction)
            {
                case Direction.Right:
                    LeftActionButton.gameObject.SetActive(true);
                    RightActionButton.gameObject.SetActive(false);
                    LeftButtonText.text = "<<";
                    InvertOrder();
                    break;
                case Direction.Left:
                    LeftActionButton.gameObject.SetActive(false);
                    RightActionButton.gameObject.SetActive(true);
                    RightButtonText.text = ">>";   
                    break;
                case Direction.None:
                    LeftActionButton.gameObject.SetActive(false);
                    RightActionButton.gameObject.SetActive(false);
                    SummaryText.GetComponent<LayoutElement>().minWidth = 300;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null);
            }
        }

        public override GameObject GetNavigationButton()
        {
             switch (Direction)
             {
                 case Direction.Right:
                     return GetLeftButton();
                 case Direction.Left:
                     return GetRightButton();
             }
            return base.GetNavigationButton();

        }

        public void OnLeftButtonPress(Action a) => LeftActionButton.AddOnClick(() => a());
        public void OnLeftButtonHold(Action a, float duration) => LeftActionButton.AddOnHold(a, duration);
        public void OnRightButtonPress(Action a) => RightActionButton.AddOnClick(() => a());
        public void OnRightButtonHold(Action a, float duration) => RightActionButton.AddOnHold(a, duration);
        public GameObject GetLeftButton() => LeftActionButton.gameObject;
        public GameObject GetRightButton() => RightActionButton.gameObject;

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            TypeText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Type");
            SummaryText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Summary");
            WeightText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Weight");
            LeftActionButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Left Button");
            RightActionButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Right Button");
            LeftButtonText = Helper.FindChildWithName<TextMeshProUGUI>(LeftActionButton.gameObject, "Text");
            RightButtonText = Helper.FindChildWithName<TextMeshProUGUI>(RightActionButton.gameObject, "Text");
            Bookends = Helper.FindChildWithName(GameObject, "Button Bookends").gameObject;
        }

        public override void Update()
        {
            BasicInventoryItem item = (BasicInventoryItem) LinkedObject;
            if (item.Quantity() == 0)
            {
                Destroy();
            }
            else
            {
                if (item is InventoryResource)
                {
                    SummaryText.text = "x" + item.Quantity();
                }
                NameText.text = item.ExtendedName();
                WeightText.text = item.TotalWeight() + "kg";
                TypeText.text = item.Type.ToString();
            }
        }
    }
}