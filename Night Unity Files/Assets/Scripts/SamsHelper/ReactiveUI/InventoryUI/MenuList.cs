using System;
using System.Collections.Generic;
using System.Linq;
using Game.Gear.UI;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class MenuList : MonoBehaviour
    {
        [HideInInspector] public readonly List<ViewParent> Items = new List<ViewParent>();
        [HideInInspector] public Transform InventoryContent;
        public int MaxDistance;
        [Range(0f, 1f)] public float MinFade = 1f;
        [Range(0, 1f)] public float UnselectedItemScale = 1f;
        public bool CentreOnSelectedItem;
        public EnhancedButton CloseButton;
        private event Action OnContentChange;

        public virtual void Awake()
        {
            InventoryContent = Helper.FindChildWithName<Transform>(gameObject, "Content");
            if (!CentreOnSelectedItem) return;
            RectTransform r = InventoryContent.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0, 0.5f);
            r.anchorMax = new Vector2(1, 0.5f);
            CloseButton.AddOnClick(MenuStateMachine.GoToInitialMenu);
        }

        public void AddOnContentChange(Action a)
        {
            OnContentChange += a;
        }

        public void Clear()
        {
            Items.ForEach(item => item.Destroy());
            Items.Clear();
        }

        public void SetItems<T>(List<T> newItems, bool autoSelectFirst = true) where T : MyGameObject
        {
            Clear();
            if (newItems.Count != 0)
            {
                newItems.ForEach(item => AddItem(item));
                if(autoSelectFirst) Items[0].PrimaryButton.Button().Select();
                return;
            }
            InventoryUi emptyButton = new InventoryUi(null, InventoryContent);
            emptyButton.SetCentralTextCallback(() => "None");
            AddPlainButton(emptyButton);
            if(autoSelectFirst) CloseButton?.Button().Select();
        }

        protected virtual void UpdateItem(MyGameObject item)
        {
            ViewParent foundItem = FindItem(item);
            if (foundItem == null) return;
            foundItem.Update();
            RefreshNavigation();
        }

        protected ViewParent FindItem(MyGameObject item)
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
                itemUi.GetGameObject().name = Items.IndexOf(itemUi).ToString();
                if (MinFade != 1f && MaxDistance != 0) itemUi.PrimaryButton.AddOnSelectEvent(() => FadeItems(itemUi));
                if (UnselectedItemScale != 1) itemUi.PrimaryButton.AddOnSelectEvent(() => ScaleItems(itemUi));
                if (CentreOnSelectedItem) itemUi.PrimaryButton.AddOnSelectEvent(() => CentreContentOnItem(itemUi));
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

        public void RefreshNavigation()
        {
            List<ViewParent> navigatableItems = Items.Where(item => item.Navigatable()).ToList();
            Items.ForEach(i => i.PrimaryButton?.ClearNavigation());
            for (int i = 1; i < navigatableItems.Count; ++i)
            {
                EnhancedButton from = navigatableItems[i - 1].PrimaryButton;
                EnhancedButton to = navigatableItems[i].PrimaryButton;
                Helper.SetReciprocalNavigation(from, to);
            }
            OnContentChange?.Invoke();
            if (CloseButton == null) return;
            ViewParent lastItem = navigatableItems.Count > 0 ? navigatableItems.Last() : null;
            if (lastItem != null)
            {
                Helper.SetReciprocalNavigation(lastItem.PrimaryButton, CloseButton);
            }
            else
            {
                CloseButton.Button().Select();
            }
        }

        public void SendToLast(InventoryUi button)
        {
            int index = Items.IndexOf(button);
            if (index == 0 && Items.Count == 1) return;
            for (int i = index + 1; i < Items.Count; ++i)
            {
                Items[i - 1] = Items[i];
            }
            Items[Items.Count - 1] = button;
            button.GetGameObject().transform.SetAsLastSibling();
        }

        private void CentreContentOnItem(ViewParent itemUi)
        {
            float targetPosition = 0;
            int itemIndex = Items.IndexOf(itemUi);
            for (int i = 0; i < Items.Count; ++i)
            {
                ViewParent otherItem = Items[i];
                float targetHeight = otherItem.GetGameObject().GetComponent<RectTransform>().rect.height;
                if (i == itemIndex - 1 && otherItem is GearUi)
                {
                    targetHeight = 40f;
                }
                if (i == itemIndex && otherItem is GearUi)
                {
                    targetHeight = 190f;
                }
                if (i == itemIndex) targetHeight /= 2;
                targetPosition += targetHeight;
                if (otherItem == itemUi) break;
            }
            RectTransform rect = InventoryContent.GetComponent<RectTransform>();
            Vector2 rectPosition = rect.anchoredPosition;
            rectPosition.y = targetPosition;
            rect.anchoredPosition = rectPosition;
        }

        private void ScaleItems(ViewParent itemUi)
        {
            foreach (ViewParent item in Items)
            {
                RectTransform rect = itemUi.GetGameObject().GetComponent<RectTransform>();
                Vector2 rectScale = new Vector2(UnselectedItemScale, UnselectedItemScale);
                if (item == itemUi) rectScale = new Vector2(1, 1);
                rect.localScale = rectScale;
            }
        }

        private void FadeItems(ViewParent itemUi)
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                float distance = Math.Abs(i - Items.IndexOf(itemUi));
                distance = Helper.Normalise(distance, MaxDistance);
                float alpha = 1f - distance;
                alpha = Mathf.Clamp(alpha, MinFade, 1);
                Items[i].PrimaryButton.GetComponent<CanvasGroup>().alpha = alpha;
            }
        }
    }
}