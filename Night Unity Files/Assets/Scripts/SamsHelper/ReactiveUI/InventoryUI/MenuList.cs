using System.Collections.Generic;
using System.Linq;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList : MonoBehaviour
    {
        protected readonly List<ViewParent> Items = new List<ViewParent>();
        protected Transform InventoryContent;

        public virtual void Awake()
        {
            InventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }

        public List<ViewParent> GetItems()
        {
            return Items;
        }

        public Transform ContentTransform()
        {
            return InventoryContent;
        }

        public virtual void SetItems(List<MyGameObject> newItems)
        {
            Items.ForEach(item => item.Destroy());
            Items.Clear();
            newItems.ForEach(item => AddItem(item));
            RefreshNavigation();
        }
        
        protected virtual ViewParent UpdateItem(MyGameObject item)
        {
            ViewParent foundItem = Items.FirstOrDefault(i => i.GetLinkedObject().Equals(item));
            foundItem?.Update();
            RefreshNavigation();
            return foundItem;
        }

        private ViewParent IsItemDisplayed(MyGameObject inventoryItem)
        {
            return Items.FirstOrDefault(itemUi => itemUi.GetLinkedObject() == inventoryItem);
        }

        public virtual ViewParent AddItem(MyGameObject item)
        {
            ViewParent existingUi = IsItemDisplayed(item);
            if (existingUi != null)
            {
                UpdateItem(item);
                return null;
            }
            ViewParent itemUi = item.CreateUi(InventoryContent);
            if (itemUi.IsDestroyed()) return null;
            Items.Add(itemUi);
            RefreshNavigation();
            return itemUi;
        }

        public void AddPlainButton(InventoryUi button)
        {
            Items.Add(button);
        }

        public void Remove(ViewParent item)
        {
            if (!Items.Contains(item)) return;
            Items.Remove(item);
            RefreshNavigation();
        }

        private List<ViewParent> GetNavigatableItems(List<ViewParent> items) => items;

        public virtual ViewParent RefreshNavigation()
        {
            List<ViewParent> navigatableItems = GetNavigatableItems(Items);
            for (int i = 0; i < navigatableItems.Count; ++i)
            {
                if (i <= 0) continue;
                GameObject from = navigatableItems[i - 1].GetNavigationButton();
                GameObject to = navigatableItems[i].GetNavigationButton();
                Helper.SetReciprocalNavigation(from, to);
            }
            return navigatableItems.Count > 0 ? navigatableItems.Last() : null;
        }

        public void SendToLast(InventoryUi exploreButton)
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