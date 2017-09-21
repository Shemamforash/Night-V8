using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList : MonoBehaviour
    {
        protected readonly List<BaseInventoryUi> Items = new List<BaseInventoryUi>();
        protected Transform InventoryContent;

        public virtual void Awake()
        {
            InventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }

        public void SetItems(List<MyGameObject> newItems)
        {
            Items.ForEach(item => item.Destroy());
            Items.Clear();
            newItems.ForEach(item =>
            {
                BaseInventoryUi uiElement = RestrictedContentCheck(item);
                if (uiElement != null)
                {
                    Items.Add(uiElement);
                }
            });
            SetNavigation();
        }

        protected virtual BaseInventoryUi UpdateItem(MyGameObject item)
        {
            BaseInventoryUi foundItem = Items.FirstOrDefault(i => i.GetLinkedObject().Equals(item));
            foundItem?.Update();
            SetNavigation();
            return foundItem;
        }

        protected virtual BaseInventoryUi RestrictedContentCheck(MyGameObject myGameObject)
        {
            return new BaseInventoryUi(myGameObject, InventoryContent);
        }
        
        protected bool IsItemDisplayed(MyGameObject inventoryItem)
        {
            return Items.Any(itemUi => itemUi.GetLinkedObject() == inventoryItem);
        }
        
        public void Add(BaseInventoryUi item)
        {
            if (IsItemDisplayed(item.GetLinkedObject())) return;
            Items.Add(item);
            SetNavigation();
        }

        public void Remove(BaseInventoryUi item)
        {
            if (!Items.Contains(item)) return;
            Items.Remove(item);
            SetNavigation();
        }

        private List<BaseInventoryUi> GetNavigatableItems(List<BaseInventoryUi> items)
        {
            List<BaseInventoryUi> navigatableItems = new List<BaseInventoryUi>();
            items.ForEach(i =>
            {
                if (i.GetButton().activeInHierarchy)
                {
                    navigatableItems.Add(i);
                }
            });
            return navigatableItems;
        }
        
        protected virtual BaseInventoryUi SetNavigation()
        {
            List<BaseInventoryUi> navigatableItems = GetNavigatableItems(Items);
            for (int i = 0; i < navigatableItems.Count; ++i)
            {
                if (i <= 0) continue;
                GameObject from = navigatableItems[i - 1].GetButton().gameObject;
                GameObject to = navigatableItems[i].GetButton().gameObject;
                Helper.SetReciprocalNavigation(from, to);
            }
            return navigatableItems.Last();
        }
    }
}