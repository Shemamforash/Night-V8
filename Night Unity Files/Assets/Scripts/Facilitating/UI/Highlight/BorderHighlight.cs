﻿using SamsHelper;
using UnityEngine;

namespace UI.Highlight
{
    public class BorderHighlight : Highlight
    {
        protected GameObject borderObject;
        protected bool initialised = false;
        public int leftOffset, rightOffset, topOffset, bottomOffset;

        public void Initialise()
        {
            borderObject =  Helper.InstantiateUiObject("Prefabs/Border", transform);
            borderObject.name = "Border";
            RectTransform rect = borderObject.GetComponent<RectTransform>();
            rect.localScale = new Vector2(1, 1);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(-leftOffset, -bottomOffset);
            rect.offsetMax = new Vector2(rightOffset, topOffset);
            initialised = true;
        }

        public void BorderOn()
        {
            if (!initialised)
            {
                Initialise();
            }
            borderObject.SetActive(true);
        }

        public void BorderOff()
        {
            borderObject.SetActive(false);
        }
    }
}