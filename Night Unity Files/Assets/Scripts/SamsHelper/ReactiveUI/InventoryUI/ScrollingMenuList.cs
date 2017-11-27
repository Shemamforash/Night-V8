using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gear.UI;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.InventoryUI
{
    public class ScrollingMenuList : MenuList
    {
        public int MaxDistance;
        [Range(0f, 1f)] public float MinFade = 1f;
        [Range(0, 1f)] public float UnselectedItemScale = 1f;
        public bool CentreOnSelectedItem;

        public override void Awake()
        {
            base.Awake();
            RectTransform rect = InventoryContent.GetComponent<RectTransform>();
            rect.pivot = CentreOnSelectedItem ? new Vector2(0.5f, 1) : new Vector2(0.5f, 0.5f);
        }

        public override ViewParent AddItem(MyGameObject item)
        {
            ViewParent itemUi = base.AddItem(item);
            itemUi.GetGameObject().name = Items.IndexOf(itemUi).ToString();
            if (MinFade != 1f && MaxDistance != 0) itemUi.PrimaryButton.AddOnSelectEvent(() => FadeItems(itemUi));
            if (UnselectedItemScale != 1) itemUi.PrimaryButton.AddOnSelectEvent(() => ScaleItems(itemUi));
            if(CentreOnSelectedItem) itemUi.PrimaryButton.AddOnSelectEvent(() => CentreContentOnItem(itemUi));
            return itemUi;
        }

        public override void SetItems(List<MyGameObject> newItems)
        {
            base.SetItems(newItems);
            Items[0].GetGameObject().GetComponent<Selectable>().Select();
        }

        private void CentreContentOnItem(ViewParent itemUi)
        {
            float targetPosition = 0;
            int itemIndex = Items.IndexOf(itemUi);
            for(int i  = 0; i < Items.Count; ++i)
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
                Items[i].PrimaryButton.GetComponent<EnhancedButton>().SetColor(new Color(1, 1, 1, alpha));
            }
        }
    }
}