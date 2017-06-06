﻿using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ManualScrollbar : Highlight
{
    public ScrollRect scrollrect;
    private bool selected = false;
    private float scrollRectYPos;

    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        selected = true;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        selected = false;
    }

    void Update()
    {
        if (selected)
        {
            float scrollAmount = Input.GetAxis("Scroll Keyboard");
            scrollRectYPos = scrollrect.verticalNormalizedPosition;
            if (scrollAmount < 0)
            {
                scrollRectYPos += 0.02f;
                if(scrollRectYPos > 1){
                    scrollRectYPos = 1;
                }
                scrollrect.verticalNormalizedPosition = scrollRectYPos;
            }
            else if (scrollAmount > 0)
            {
                scrollRectYPos -= 0.02f;
                if(scrollRectYPos < 0){
                    scrollRectYPos = 0;
                }
                scrollrect.verticalNormalizedPosition = scrollRectYPos;
            }
        }
    }
}
