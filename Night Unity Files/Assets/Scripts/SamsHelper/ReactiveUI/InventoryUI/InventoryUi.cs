using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryUi : SimpleView
    {
        private EnhancedButton _leftButton, _rightButton;
        private TextMeshProUGUI _leftButtonText, _rightButtonText;
        private TextMeshProUGUI _leftText, _rightText;

        private Func<string> _leftButtonTextCallback, _rightButtonTextCallback, _leftTextCallback, _rightTextCallback;
        protected Direction Direction;
        protected GameObject Bookends;


        public InventoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/FlexibleItem") : base(linkedObject, parent, prefabLocation)
        {
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            _rightText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Right Text");
            _rightText.gameObject.SetActive(false);
            _leftText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Left Text");
            _leftText.gameObject.SetActive(false);

            _leftButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Left Button");
            _leftButton.gameObject.SetActive(false);
            _rightButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Right Button");
            _rightButton.gameObject.SetActive(false);

            _leftButtonText = Helper.FindChildWithName<TextMeshProUGUI>(_leftButton.gameObject, "Text");
            _rightButtonText = Helper.FindChildWithName<TextMeshProUGUI>(_rightButton.gameObject, "Text");

            Bookends = Helper.FindChildWithName(GameObject, "Bookends").gameObject;
        }

        public override void Update()
        {
            base.Update();
            UpdateText(_leftText, _leftTextCallback);
            UpdateText(_rightText, _rightTextCallback);
            UpdateText(_leftButtonText, _leftButtonTextCallback);
            UpdateText(_rightButtonText, _rightButtonTextCallback);
        }

        private void InvertOrder()
        {
            Func<string> newLeftTextCallback = _rightTextCallback;
            Func<string> newRightTextCallback = _leftTextCallback;
            Func<string> newLeftButtonTextCallback = _leftButtonTextCallback;
            Func<string> newRightButtonTextCallback = _rightButtonTextCallback;
            SetLeftTextCallback(newLeftTextCallback);
            SetRightTextCallback(newRightTextCallback);
            SetLeftButtonTextCallback(newLeftButtonTextCallback);
            SetRightButtonTextCallback(newRightButtonTextCallback);
        }

        public override GameObject GetNavigationButton()
        {
            switch (Direction)
            {
                case Direction.Right:
                    return GetLeftButton();
                case Direction.Left:
                    return GetRightButton();
                default:
                    return base.GetNavigationButton();
            }
        }

        public void SetDirection(Direction direction)
        {
//            Direction = direction;
            switch (direction)
            {
                case Direction.Right:
                    SetLeftButtonTextCallback(() => "<<");
                    InvertOrder();
                    break;
                case Direction.Left:
                    SetRightButtonTextCallback(() => ">>");
                    break;
            }
        }

        //Button activation
        public void SetLeftButtonActive(bool active) => _leftButton.GetComponent<Button>().enabled = active;

        public void SetRightButtonActive(bool active) => _leftButton.GetComponent<Button>().enabled = active;

        //Text Callbacks
        public void SetLeftButtonTextCallback(Func<string> a)
        {
            if (a == null) return;
            _leftButton.gameObject.SetActive(true);
            _leftButtonText.gameObject.SetActive(true);
            _leftButtonTextCallback = a;
            Update();
        }


        public void SetRightButtonTextCallback(Func<string> a)
        {
            if (a == null) return;
            _rightButton.gameObject.SetActive(true);
            _rightButtonText.gameObject.SetActive(true);
            _rightButtonTextCallback = a;
            Update();
        }

        public void SetLeftTextCallback(Func<string> a)
        {
            if (a == null) return;
            _leftText.gameObject.SetActive(true);
            _leftTextCallback = a;
            Update();
        }

        public void SetRightTextCallback(Func<string> a)
        {
            if (a == null) return;
            _rightText.gameObject.SetActive(true);
            _rightTextCallback = a;
            Update();
        }

        //Button presses
        public void OnLeftButtonPress(Action a) => _leftButton.AddOnClick(() => a());

        public void OnLeftButtonHold(Action a, float duration) => _leftButton.AddOnHold(a, duration);
        public void OnRightButtonPress(Action a) => _rightButton.AddOnClick(() => a());
        public void OnRightButtonHold(Action a, float duration) => _rightButton.AddOnHold(a, duration);

        //Getters
        public GameObject GetLeftButton() => _leftButton.gameObject;

        public GameObject GetRightButton() => _rightButton.gameObject;

        public void SetLeftTextWidth(int i)
        {
            _leftText.GetComponent<LayoutElement>().minWidth = i;
        }
    }
}