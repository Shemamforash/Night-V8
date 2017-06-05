using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class ManualScrollbar : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public ScrollRect scrollrect;
    private bool selected = false;

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    private float scrollRectYPos;

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
