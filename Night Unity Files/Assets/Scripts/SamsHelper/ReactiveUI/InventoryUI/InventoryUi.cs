using System;
using SamsHelper.BaseGameFunctionality.Basic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class InventoryUi : SimpleView
    {
        private TextMeshProUGUI _leftText, _rightText;

        private Func<string> _leftTextCallback, _rightTextCallback;

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
        }

        public override void Update()
        {
            base.Update();
            UpdateText(_leftText, _leftTextCallback);
            UpdateText(_rightText, _rightTextCallback);
        }

        private void InvertOrder()
        {
            Func<string> newLeftTextCallback = _rightTextCallback;
            Func<string> newRightTextCallback = _leftTextCallback;
            SetLeftTextCallback(newLeftTextCallback);
            SetRightTextCallback(newRightTextCallback);
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
        
        public void SetLeftTextWidth(int i)
        {
            _leftText.GetComponent<LayoutElement>().minWidth = i;
        }
    }
}