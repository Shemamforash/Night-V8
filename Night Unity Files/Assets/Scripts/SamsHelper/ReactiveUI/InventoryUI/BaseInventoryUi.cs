using System;
using Articy.Night;
using Articy.Unity;
using Facilitating.UI.Elements;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.Basic;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class BaseInventoryUi
    {
        protected readonly GameObject GameObject;
        protected EnhancedButton PrimaryActionButton;
        protected readonly MyGameObject LinkedObject;

        public BaseInventoryUi(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/DefaultItem")
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            CacheUiElements();
            Update();
        }

        protected virtual void CacheUiElements()
        {
            PrimaryActionButton = GameObject.GetComponent<EnhancedButton>();
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

        public void Destroy() => GameObject.Destroy(GameObject);
        
        public virtual void Update()
        {
        }

        public void OnActionPress(Action a) => PrimaryActionButton.AddOnClick(() => a());
        public void OnActionHold(Action a, float duration) => PrimaryActionButton.AddOnHold(a, duration);
        public GameObject GetButton() => PrimaryActionButton.gameObject;
        public virtual GameObject GetNavigationButton() => PrimaryActionButton.gameObject;
        public MyGameObject GetLinkedObject() => LinkedObject;
    }
}