using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public abstract class ViewParent
    {
        protected readonly MyGameObject LinkedObject;
        protected readonly GameObject GameObject;
        private Func<bool> _destroyCheck;
        private bool _isDestroyed;
        private EnhancedButton _primaryButton;

        protected ViewParent(MyGameObject linkedObject, Transform parent, string prefabLocation)
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            CacheUiElements();
            Update();
        }

        protected virtual void CacheUiElements()
        {
            _primaryButton = GameObject.GetComponent<EnhancedButton>();
        }

        public virtual void Update()
        {
            if (_destroyCheck == null || !_destroyCheck()) return;
            _isDestroyed = true;
            Destroy();
        }

        public void Destroy()
        {
            Object.Destroy(GameObject);
            _isDestroyed = true;
        }

        public virtual GameObject GetNavigationButton() =>_primaryButton.gameObject;
        public bool IsDestroyed() => _isDestroyed;
        public void SetDestroyCondition(Func<bool> destroyCheck) => _destroyCheck = destroyCheck;
        public void SetPreferredHeight(float height) => GameObject.GetComponent<LayoutElement>().preferredHeight = height;
        public void OnPress(Action a) => _primaryButton.AddOnClick(() => a());
        //Misc
        public void DisableBorder() => _primaryButton.DisableBorder();
        public void OnHold(Action a, float duration) => _primaryButton.AddOnHold(a, duration);
        public void OnEnter(Action a) => _primaryButton.AddOnSelectEvent(a);
        protected void OnExit(Action a) => _primaryButton.AddOnDeselectEvent(a);
        public GameObject GetGameObject() => GameObject;
        public MyGameObject GetLinkedObject() => LinkedObject;
    }
}