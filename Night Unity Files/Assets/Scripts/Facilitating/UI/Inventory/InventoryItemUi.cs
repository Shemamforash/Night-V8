﻿using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Facilitating.UI.Elements;
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
        protected EnhancedButton LeftActionButton, RightActionButton;
        protected TextMeshProUGUI NameText, LeftButtonText, RightButtonText;
        protected TextMeshProUGUI TypeText, WeightText;
        protected TextMeshProUGUI SummaryText;
        protected GameObject Bookends;
        protected readonly Direction Direction;

        public InventoryItemUi(BasicInventoryItem item, Transform parent, Direction direction = Direction.None) : base(item, parent)
        {
            Direction = direction;
            SetActionButton();
            GameObject.GetComponent<LayoutElement>().preferredHeight = 40;
        }

        private void SetActionButton()
        {
            switch (Direction)
            {
                case Direction.Left:
                    LeftActionButton.gameObject.SetActive(true);
                    RightActionButton.gameObject.SetActive(false);
                    LeftButtonText.text = ">>";
                    break;
                case Direction.Right:
                    LeftActionButton.gameObject.SetActive(false);
                    RightActionButton.gameObject.SetActive(true);
                    RightButtonText.text = "<<";                    
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
                 case Direction.Left:
                     return GetLeftButton();
                 case Direction.Right:
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
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
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