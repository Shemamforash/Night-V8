using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class SimpleView : ViewParent
    {
        private TextMeshProUGUI _centralText;
        private Func<string> _centralTextCallback;

        public SimpleView(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/SimpleItem") : base(linkedObject, parent, prefabLocation)
        {
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            _centralText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Centre Text");
            _centralText.gameObject.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            UpdateText(_centralText, _centralTextCallback);
        }

        protected void UpdateText(TextMeshProUGUI textMesh, Func<string> textCallback)
        {
            if (textCallback == null)
                textMesh.gameObject.SetActive(false);
            else
                textMesh.text = textCallback();
        }

        public void SetCentralTextCallback(Func<string> a)
        {
            if (a == null) return;
            _centralText.gameObject.SetActive(true);
            _centralTextCallback = a;
            Update();
        }
    }
}