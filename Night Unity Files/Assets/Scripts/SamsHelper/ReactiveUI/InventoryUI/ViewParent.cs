using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public abstract class ViewParent
    {
        protected readonly GameObject GameObject;
        protected readonly MyGameObject LinkedObject;
        private Func<bool> _destroyCondition;
        private bool _isDestroyed;
        private MenuList _menuList;
        private bool _navigatable = true;

        protected ViewParent(MyGameObject linkedObject, Transform parent, string prefabLocation)
        {
            LinkedObject = linkedObject;
            GameObject = Helper.InstantiateUiObject(prefabLocation, parent);
            if (linkedObject != null) GameObject.name = linkedObject.Name;
            CacheUiElements();
            Update();
        }

        public EnhancedButton PrimaryButton { get; private set; }

        public void SetMenuList(MenuList menuList)
        {
            _menuList = menuList;
        }

        public void SetNavigatable(bool navigatable)
        {
            if (_navigatable == navigatable) return;
            _navigatable = navigatable;
            _menuList?.RefreshNavigation();
        }

        public bool Navigatable()
        {
            return _navigatable;
        }

        protected virtual void CacheUiElements()
        {
            PrimaryButton = GameObject.GetComponent<EnhancedButton>();
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
            Object.Destroy(GameObject);
            _isDestroyed = true;
        }

        public bool IsDestroyed()
        {
            return _isDestroyed;
        }

        public void SetDestroyCondition(Func<bool> destroyCheck)
        {
            _destroyCondition = destroyCheck;
        }

        public void SetPreferredHeight(float height)
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = height;
        }

        //Misc
        public GameObject GetGameObject()
        {
            return GameObject;
        }

        public MyGameObject GetLinkedObject()
        {
            return LinkedObject;
        }

        public void Select()
        {
            PrimaryButton.Button().Select();
        }
    }
}