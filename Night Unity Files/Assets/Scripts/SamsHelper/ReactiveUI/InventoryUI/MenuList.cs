using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList<T> : MonoBehaviour where T : SimpleItemUi
    {
        protected List<T> _items = new List<T>();
        protected Transform _inventoryContent;

        public virtual void Awake()
        {
            _inventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }
        
        public void AddItems(List<T> items)
        {
            _items.ForEach(i => i.Destroy());
            _items.Clear();
            _items.AddRange(items);
            SetNavigation();
        }

        public void RemoveItem(T item)
        {
            if (!_items.Contains(item)) return;
            _items.Remove(item);
            SetNavigation();
        }

        private List<T> GetNavigatableItems(List<T> items)
        {
            List<T> navigatableItems = new List<T>();
            items.ForEach(i =>
            {
                if (i.GetButton().activeInHierarchy)
                {
                    navigatableItems.Add(i);
                }
            });
            return navigatableItems;
        }
        
        protected virtual T SetNavigation()
        {
            List<T> navigatableItems = GetNavigatableItems(_items);
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