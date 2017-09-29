using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryUi
    {
        private EnhancedButton _leftButton, _rightButton, _primaryButton;
        private TextMeshProUGUI _leftButtonText, _rightButtonText;
        private TextMeshProUGUI _leftText, _centralText, _rightText;

        private Func<string> _leftButtonTextCallback, _rightButtonTextCallback, _leftTextCallback, _rightTextCallback, _centralTextCallback;
        protected Direction Direction;
        protected GameObject Bookends;

        protected readonly GameObject GameObject;
        protected readonly MyGameObject LinkedObject;
        private Func<bool> _destroyCheck;

        public InventoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/FlexibleItem")
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            CacheUiElements();
            Update();
        }

        protected void CacheUiElements()
        {
            _rightText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Right Text");
            _rightText.gameObject.SetActive(false);
            _leftText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Left Text");
            _leftText.gameObject.SetActive(false);
            _centralText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Centre Text");
            _centralText.gameObject.SetActive(false);

            _primaryButton = GameObject.GetComponent<EnhancedButton>();
            _leftButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Left Button");
            _leftButton.gameObject.SetActive(false);
            _rightButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Right Button");
            _rightButton.gameObject.SetActive(false);

            _leftButtonText = Helper.FindChildWithName<TextMeshProUGUI>(_leftButton.gameObject, "Text");
            _rightButtonText = Helper.FindChildWithName<TextMeshProUGUI>(_rightButton.gameObject, "Text");

            Bookends = Helper.FindChildWithName(GameObject, "Bookends").gameObject;
        }

        public void Update()
        {
            if (_destroyCheck != null && _destroyCheck())
            {
                Destroy();
            }
            UpdateText(_centralText, _centralTextCallback);
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

        private void UpdateText(TextMeshProUGUI textMesh, Func<string> textCallback)
        {
            if (textCallback == null)
            {
                textMesh.gameObject.SetActive(false);
            }
            else
            {
                textMesh.text = textCallback();
            }
        }

        public GameObject GetNavigationButton()
        {
            switch (Direction)
            {
                case Direction.Right:
                    return GetLeftButton();
                case Direction.Left:
                    return GetRightButton();
                default:
                    return _primaryButton.gameObject;
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

        //Misc
        public void DisableBorder() => _primaryButton.DisableBorder();

        public void Destroy() => Object.Destroy(GameObject);
        public void SetDestroyCondition(Func<bool> destroyCheck) => _destroyCheck = destroyCheck;
        public void SetPreferredHeight(float height) => GameObject.GetComponent<LayoutElement>().preferredHeight = height;

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

        public void SetCentralTextCallback(Func<string> a)
        {
            if (a == null) return;
            _centralText.gameObject.SetActive(true);
            _centralTextCallback = a;
            Update();
        }


        //Button presses
        public void OnLeftButtonPress(Action a) => _leftButton.AddOnClick(() => a());

        public void OnLeftButtonHold(Action a, float duration) => _leftButton.AddOnHold(a, duration);
        public void OnRightButtonPress(Action a) => _rightButton.AddOnClick(() => a());
        public void OnRightButtonHold(Action a, float duration) => _rightButton.AddOnHold(a, duration);
        public void OnPress(Action a) => _primaryButton.AddOnClick(() => a());
        public void OnHold(Action a, float duration) => _primaryButton.AddOnHold(a, duration);
        public void OnHover(Action a) => _primaryButton.AddOnSelectEvent(a);

        //Getters
        public GameObject GetLeftButton() => _leftButton.gameObject;

        public GameObject GetRightButton() => _rightButton.gameObject;
        public MyGameObject GetLinkedObject() => LinkedObject;
        public GameObject GetGameObject() => GameObject;
    }
}