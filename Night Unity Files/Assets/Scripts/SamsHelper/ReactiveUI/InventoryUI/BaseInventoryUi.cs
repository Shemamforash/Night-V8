using System;
using Articy.Night;
using Facilitating.UI.Elements;
using SamsHelper.BaseGameFunctionality;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class BaseInventoryUi
    {
        protected readonly GameObject GameObject;
        protected EnhancedButton ActionButton;
        protected TextMeshProUGUI NameText;
        protected readonly MyGameObject LinkedObject;

        public BaseInventoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/DefaultItem")
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            CacheUiElements();
        }

        protected virtual void CacheUiElements()
        {
            ActionButton = Helper.FindChildWithName<EnhancedButton>(GameObject, "Action Button");
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
        }

        public void Destroy()
        {
            GameObject.Destroy(GameObject);
        }

        public virtual void Update()
        {
        }

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

        public MyGameObject GetLinkedObject()
        {
            return LinkedObject;
        }
    }
}