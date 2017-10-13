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
        public override InventoryUi AddItem(MyGameObject item)
        {
            InventoryUi itemUi = base.AddItem(item);
            itemUi?.OnEnter(() =>
            {
                CentreContentOnItem(itemUi);
                FadeOtherItems(itemUi);
            });
            return itemUi;
        }

        public override void SetItems(List<MyGameObject> newItems)
        {
            base.SetItems(newItems);
            Items[0].GetGameObject().GetComponent<Selectable>().Select();
        }

        private void CentreContentOnItem(InventoryUi itemUi)
        {
            float itemYPosition = itemUi.GetGameObject().GetComponent<RectTransform>().anchoredPosition.y;
            RectTransform rect = InventoryContent.GetComponent<RectTransform>();
            Vector2 rectPosition = rect.anchoredPosition;
            rectPosition.y = -itemYPosition + itemUi.GetGameObject().GetComponent<RectTransform>().rect.height / 2;
            rect.anchoredPosition = rectPosition;
        }

        private void FadeOtherItems(InventoryUi itemUi)
        {
            for (int i = 0; i < Items.Count; ++i)
            {
                float maxDistance = 6;
                if (Items[i].GetGameObject() == null) continue;
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
    }
}