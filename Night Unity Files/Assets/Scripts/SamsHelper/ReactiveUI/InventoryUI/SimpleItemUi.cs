using System;
using Facilitating.UI.Elements;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public abstract class SimpleItemUi
    {
        protected readonly GameObject _gameObject;
        protected readonly EnhancedButton ActionButton;
        protected readonly TextMeshProUGUI _typeText, _nameText, _weightText;
        protected readonly TextMeshProUGUI SummaryText, ButtonText;

        public SimpleItemUi(Transform parent)
        {
            _gameObject = Helper.InstantiateUiObject("Prefabs/Inventory Button", parent);
            ActionButton = Helper.FindChildWithName<EnhancedButton>(_gameObject, "Action Button");
            _typeText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Type");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Name");
            SummaryText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Summary");
            _weightText = Helper.FindChildWithName<TextMeshProUGUI>(_gameObject, "Weight");
            ButtonText = Helper.FindChildWithName<TextMeshProUGUI>(ActionButton.gameObject, "Text");
        }
        
        public void Destroy()
        {
            GameObject.Destroy(_gameObject);
        }

        public abstract void Update();
        
        public void OnActionPress(Action a)
        {
            ActionButton.AddOnClick(() => a());
        }

        public void OnActionHold(Action a, float duration)
        {
            ActionButton.AddOnHold(a, duration);
        }

        public GameObject GetButton()
        {
            return ActionButton.gameObject;
        }
    }
}