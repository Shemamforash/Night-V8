using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList : MonoBehaviour
    {
        private readonly List<InventoryUi> Items = new List<InventoryUi>();
        protected Transform InventoryContent;
        private bool _fadeFromCenter;

        public virtual void Awake()
        {
            InventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }

        public void EnableFadeFromCenter()
        {
            _fadeFromCenter = true;
        }

        public List<InventoryUi> GetItems()
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
            newItems.ForEach(item => AddItem(item));
            RefreshNavigation();
        }

        protected virtual InventoryUi UpdateItem(MyGameObject item)
        {
            InventoryUi foundItem = Items.FirstOrDefault(i => i.GetLinkedObject().Equals(item));
            foundItem?.Update();
            RefreshNavigation();
            return foundItem;
        }

        private InventoryUi IsItemDisplayed(MyGameObject inventoryItem)
        {
            return Items.FirstOrDefault(itemUi => itemUi.GetLinkedObject() == inventoryItem);
        }

        public virtual InventoryUi AddItem(MyGameObject item)
        {
            InventoryUi existingUi = IsItemDisplayed(item);
            if (existingUi != null)
            {
                UpdateItem(item);
                return null;
            }
            InventoryUi itemUi = item.CreateUi(InventoryContent);
            Items.Add(itemUi);
            itemUi.OnEnter(() =>
            {
                float itemYPosition = itemUi.GetGameObject().GetComponent<RectTransform>().anchoredPosition.y;
                RectTransform rect = InventoryContent.GetComponent<RectTransform>();
                Vector2 rectPosition = rect.anchoredPosition;
                rectPosition.y = -itemYPosition + itemUi.GetGameObject().GetComponent<RectTransform>().rect.height / 2;
                rect.anchoredPosition = rectPosition;
                for (int i = 0; i < Items.Count; ++i)
                {
                    float maxDistance = 6;
                    if (Items[i].GetGameObject() != null)
                    {
//                        float distance = Math.Abs(Items[i].GetGameObject().GetComponent<RectTransform>().anchoredPosition.y + rectPosition.y);
                        float distance = Math.Abs(i - Items.IndexOf(itemUi));
                        float alpha = 1 - distance / maxDistance;
                        if (alpha < 0f)
                        {
                            alpha = 0f;
                        }
                        Items[i].GetGameObject().GetComponent<EnhancedButton>().ChangeTextColor(new Color(1, 1, 1, alpha));
                    }
                }
            });
            RefreshNavigation();
            return itemUi;
        }

        public void AddPlainButton(InventoryUi button)
        {
            Items.Add(button);
        }

        public void Remove(InventoryUi item)
        {
            if (!Items.Contains(item)) return;
            Items.Remove(item);
            RefreshNavigation();
        }

        private List<InventoryUi> GetNavigatableItems(List<InventoryUi> items) => items;

        public virtual InventoryUi RefreshNavigation()
        {
            List<InventoryUi> navigatableItems = GetNavigatableItems(Items);
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