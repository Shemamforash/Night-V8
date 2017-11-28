using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList : MonoBehaviour
    {
        [HideInInspector]
        public readonly List<ViewParent> Items = new List<ViewParent>();
        [HideInInspector]
        public Transform InventoryContent;

        public virtual void Awake()
        {
            InventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
        }

        private void Clear()
        {
            Items.ForEach(item => item.Destroy());
            Items.Clear();
        }

        public virtual void SetItems(List<MyGameObject> newItems)
        {
            Clear();
            newItems.ForEach(item => AddItem(item));
        }

        protected virtual void UpdateItem(MyGameObject item)
        {
            ViewParent foundItem = FindItem(item);
            if (foundItem == null) return;
            foundItem.Update();
            RefreshNavigation();
        }

        private ViewParent FindItem(MyGameObject item)
        {
            return Items.FirstOrDefault(i => i.GetLinkedObject() != null && i.GetLinkedObject().Equals(item));
        }

        public virtual ViewParent AddItem(MyGameObject item)
        {
            ViewParent existingUi = FindItem(item);
            InventoryItem inventoryItem = item as InventoryItem;
            
            if (existingUi != null)
            {
                if (inventoryItem == null || !inventoryItem.IsStackable()) throw new Exceptions.ItemAlreadyExistsInMenuListException(item, gameObject);
                existingUi.Update();
                return existingUi;
            }
            ViewParent itemUi = item.CreateUi(InventoryContent);
            if (itemUi != null)
            {
                itemUi.SetMenuList(this);
                Items.Add(itemUi);
                RefreshNavigation();
            }
            return itemUi;
        }

        public void AddPlainButton(InventoryUi button)
        {
            Items.Add(button);
        }

        public void Remove(ViewParent item)
        {
            if (!Items.Contains(item)) throw new Exceptions.TryRemoveItemDoesNotExistException(item.GetLinkedObject(), gameObject);
            Items.Remove(item);
            item.Destroy();
            RefreshNavigation();
        }

        public virtual ViewParent RefreshNavigation()
        {
            List<ViewParent> navigatableItems = Items.Where(item => item.Navigatable()).ToList();
            Items.ForEach(i => Helper.ClearNavigation(i.GetNavigationButton()));
            for (int i = 1; i < navigatableItems.Count; ++i)
            {
                Button from = navigatableItems[i - 1].GetNavigationButton();
                Button to = navigatableItems[i].GetNavigationButton();
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