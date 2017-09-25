using System;
using System.Collections.Generic;
using System.Linq;
using Facilitating.UI.Inventory;
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

        public Transform ContentTransform()
        {
            return InventoryContent;
        }

        public void SetItems(List<MyGameObject> newItems)
        {
            Items.ForEach(item => item.Destroy());
            Items.Clear();
            newItems.ForEach(item =>
            {
                BaseInventoryUi uiElement = item.CreateUi(InventoryContent);
                if (uiElement != null)
                {
                    Add(uiElement);
                }
            });
            RefreshNavigation();
        }

        protected virtual BaseInventoryUi UpdateItem(MyGameObject item)
        {
            BaseInventoryUi foundItem = Items.FirstOrDefault(i => i.GetLinkedObject().Equals(item));
            foundItem?.Update();
            RefreshNavigation();
            return foundItem;
        }

        private bool IsItemDisplayed(MyGameObject inventoryItem)
        {
            return Items.Any(itemUi => itemUi.GetLinkedObject() == inventoryItem);
        }

        public virtual BaseInventoryUi Add(BaseInventoryUi item)
        {
            if (IsItemDisplayed(item.GetLinkedObject()))
            {
                Debug.Log(item.GetLinkedObject().Name + "    NAMAMAMA");
                Items.ForEach(itemUi => Debug.Log(item.GetLinkedObject().Name));
                item.Destroy();
                return null;
            }
            Items.Add(item);
            RefreshNavigation();
            return item;
        }

        public BaseInventoryUi Add(MyGameObject item)
        {
            BaseInventoryUi itemUi = item.CreateUi(InventoryContent);
            return Add(itemUi);
        }

        protected void Remove(BaseInventoryUi item)
        {
            if (!Items.Contains(item)) return;
            Items.Remove(item);
            RefreshNavigation();
        }

        protected virtual List<BaseInventoryUi> GetNavigatableItems(List<BaseInventoryUi> items) => items;

        public virtual BaseInventoryUi RefreshNavigation()
        {
            List<BaseInventoryUi> navigatableItems = GetNavigatableItems(Items);
            for (int i = 0; i < navigatableItems.Count; ++i)
            {
                if (i <= 0) continue;
                GameObject from = navigatableItems[i - 1].GetNavigationButton();
                GameObject to = navigatableItems[i].GetNavigationButton();
                Helper.SetReciprocalNavigation(from, to);
            }
            return navigatableItems.Count > 0 ? navigatableItems.Last() : null;
        }

        public void SendToLast(BaseInventoryUi exploreButton)
        {
            int index = Items.IndexOf(exploreButton);
            for (int i = index + 1; i < Items.Count; ++i)
            {
                Items[i - 1] = Items[i];
            }
            Items[Items.Count - 1] = exploreButton;
            exploreButton.GetGameObject().transform.SetAsLastSibling();
        }
    }
}