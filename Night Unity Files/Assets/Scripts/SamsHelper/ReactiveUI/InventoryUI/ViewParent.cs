using System;
using System.Collections.Specialized;
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
        private Func<bool> _destroyCondition;
        private bool _isDestroyed;
        private EnhancedButton _primaryButton;
        private bool _navigatable = true;
        private MenuList _menuList;

        protected ViewParent(MyGameObject linkedObject, Transform parent, string prefabLocation)
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            CacheUiElements();
            Update();
        }

        public void SetMenuList(MenuList menuList)
        {
            _menuList = menuList;
        }

        protected void SetNavigatable(bool navigatable)
        {
            _navigatable = navigatable;
            _menuList?.RefreshNavigation();
        }

        public bool Navigatable()
        {
            return _navigatable;
        }
        
        protected virtual void CacheUiElements()
        {
            _primaryButton = GameObject.GetComponent<EnhancedButton>();
        }

        private void CheckToDestroy()
        {
            if (_destroyCondition == null || !_destroyCondition()) return;
            Destroy();
        }

        public virtual void Update()
        {
            CheckToDestroy();
        }

        public void Destroy()
        {
            GameObject.Destroy(GameObject);
            _isDestroyed = true;
        }

        public bool IsDestroyed() => _isDestroyed;
        public void SetDestroyCondition(Func<bool> destroyCheck) => _destroyCondition = destroyCheck;
        public void SetPreferredHeight(float height) => GameObject.GetComponent<LayoutElement>().preferredHeight = height;
        public EnhancedButton PrimaryButton => _primaryButton;
        //Misc
        public GameObject GetGameObject() => GameObject;
        public MyGameObject GetLinkedObject() => LinkedObject;

        public void Select()
        {
            _primaryButton.Button().Select();
        }
    }
}