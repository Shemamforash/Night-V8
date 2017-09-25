using System;
using Articy.Night;
using Articy.Unity;
using Facilitating.UI.Elements;
using Game.Characters.CharacterActions;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class BaseInventoryUi
    {
        protected readonly GameObject GameObject;
        protected EnhancedButton PrimaryActionButton;
        protected readonly MyGameObject LinkedObject;
        protected TextMeshProUGUI NameText;
        private string _defaultText = "New List Element";

        public BaseInventoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/SimpleItem")
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            CacheUiElements();
            Update();
        }

        public void DisableBorder()
        {
            PrimaryActionButton.DisableBorder();
        }

        protected virtual void CacheUiElements()
        {
            PrimaryActionButton = GameObject.GetComponent<EnhancedButton>();
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
        }

        public void InvertOrder()
        {
            int noChildren = GameObject.transform.childCount;
            for (int i = 0; i < noChildren; ++i)
            {
                int mirrorOpposite = noChildren - i - 1;
                if (mirrorOpposite <= i)
                {
                    break;
                }
                Transform oppositeChild = GameObject.transform.GetChild(mirrorOpposite);
                Transform child = GameObject.transform.GetChild(i);
                oppositeChild.SetSiblingIndex(i);
                child.SetSiblingIndex(mirrorOpposite);
            }
        }

        public GameObject GetGameObject() => GameObject;

        public void Destroy() => GameObject.Destroy(GameObject);

        public virtual void Update()
        {
            NameText.text = LinkedObject == null ? _defaultText : LinkedObject.Name;
        }

        public void OnActionPress(Action a) => PrimaryActionButton.AddOnClick(() => a());
        public void OnActionHold(Action a, float duration) => PrimaryActionButton.AddOnHold(a, duration);
        public virtual GameObject GetNavigationButton() => PrimaryActionButton.gameObject;
        public MyGameObject GetLinkedObject() => LinkedObject;

        public void SetDefaultText(string defaultText)
        {
            _defaultText = defaultText;
            NameText.text = _defaultText;
        }
    }
}