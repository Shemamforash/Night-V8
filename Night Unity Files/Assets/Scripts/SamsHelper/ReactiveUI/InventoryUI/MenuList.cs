using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList : MonoBehaviour
    {
        private readonly List<BaseInventoryUi> Items = new List<BaseInventoryUi>();
        protected Transform InventoryContent;

        public virtual void Awake()
        {
            InventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }

        public List<BaseInventoryUi> GetItems()
        {
            return Items;
        }

        public void SetItems(List<MyGameObject> newItems, Func<MyGameObject, BaseInventoryUi> uiCreationMethod = null)
        {
            Items.ForEach(item => item.Destroy());
            Items.Clear();
            newItems.ForEach(item =>
            {
                BaseInventoryUi uiElement = uiCreationMethod == null ? new BaseInventoryUi(item, InventoryContent) : uiCreationMethod(item);
                if (uiElement != null)
                {
                    Add(uiElement);
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

        private bool IsItemDisplayed(MyGameObject inventoryItem)
        {
            return Items.Any(itemUi => itemUi.GetLinkedObject() == inventoryItem);
        }

        protected virtual void Add(BaseInventoryUi item)
        {
            if (IsItemDisplayed(item.GetLinkedObject())) return;
            Items.Add(item);
            SetNavigation();
        }

        protected void Remove(BaseInventoryUi item)
        {
            if (!Items.Contains(item)) return;
            Items.Remove(item);
            SetNavigation();
        }

        protected virtual List<BaseInventoryUi> GetNavigatableItems(List<BaseInventoryUi> items) => items;
        
        protected virtual BaseInventoryUi SetNavigation()
        {
            List<BaseInventoryUi> navigatableItems = GetNavigatableItems(Items);
            for (int i = 0; i < navigatableItems.Count; ++i)
            {
                if (i <= 0) continue;
                GameObject from = navigatableItems[i - 1].GetNavigationButton();
                GameObject to = navigatableItems[i].GetNavigationButton();
                Helper.SetReciprocalNavigation(from, to);
            }
            return navigatableItems.Last();
        }
    }
}