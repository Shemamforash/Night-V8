using System;
using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class ScrollingMenuList : MenuList
    {
        public int MaxDistance = 6;
        public float MinFade;
        public float UnselectedItemScale = 1f;
        private Action<ViewParent, bool> _unselectedItemAction;

        public override void Awake()
        {
            base.Awake();
            RectTransform rect = ContentTransform().GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 1);
        }

        public override ViewParent AddItem(MyGameObject item)
        {
            ViewParent itemUi = base.AddItem(item);
            itemUi.GetGameObject().name = GetItems().IndexOf(itemUi).ToString();
            itemUi.OnEnter(() =>
            {
                FadeOtherItems(itemUi);
                CentreContentOnItem(itemUi);
            });
            return itemUi;
        }

        public void SetUnselectedItemAction(Action<ViewParent, bool> a) => _unselectedItemAction = a;

        public override void SetItems(List<MyGameObject> newItems)
        {
            base.SetItems(newItems);
            Items[0].GetGameObject().GetComponent<Selectable>().Select();
        }

        private void CentreContentOnItem(ViewParent itemUi)
        {
            float targetPosition = 0;
            foreach (ViewParent otherItem in GetItems())
            {
                float targetHeight = otherItem.GetGameObject().GetComponent<LayoutElement>().preferredHeight;
                if (otherItem == itemUi)
                {
                    targetPosition += targetHeight / 2;
                    break;
                }
                if (otherItem.GetGameObject().activeInHierarchy)
                {
                    targetPosition += targetHeight;
                }
            }
            RectTransform rect = InventoryContent.GetComponent<RectTransform>();
            Vector2 rectPosition = rect.anchoredPosition;
            rectPosition.y = targetPosition;
            rect.anchoredPosition = rectPosition;
        }

        private void ScaleItem(ViewParent itemUi, bool isSelected)
        {
            RectTransform rect = itemUi.GetGameObject().GetComponent<RectTransform>();
            if (isSelected)
            {
                rect.localScale = new Vector2(1, 1);
                return;
            }
            rect.localScale = new Vector2(UnselectedItemScale, UnselectedItemScale);
        }
        
        private void FadeOtherItems(ViewParent itemUi)
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                GameObject itemObject = Items[i].GetGameObject();
                if (itemObject == null) continue;
                int distance = Math.Abs(i - Items.IndexOf(itemUi));
                ScaleItem(Items[i], distance == 0);
                _unselectedItemAction?.Invoke(Items[i], distance == 0);
                float alpha = 1f - (float) distance / MaxDistance;
                if (alpha < MinFade)
                {
                    alpha = MinFade;
                }
                if (alpha == 0 && itemObject.activeInHierarchy)
                {
                    itemObject.SetActive(false);
                }
                else if(alpha != 0)
                {
                    if (!itemObject.activeInHierarchy)
                    {
                        itemObject.SetActive(true);    
                    }
                    itemObject.GetComponent<EnhancedButton>().ChangeTextColor(new Color(1, 1, 1, alpha));
                }
            }
        }
    }
}