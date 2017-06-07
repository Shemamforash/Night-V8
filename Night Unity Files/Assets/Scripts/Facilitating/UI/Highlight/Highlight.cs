using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class Highlight : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    /*Base class for all interactive objects.
    If it's interactive, it must have it's colours inverted when selected, and must have a tooltip.
    */

    protected Text buttonText;
    public string tooltipText;

    public virtual string GetTooltip()
    {
        return tooltipText;
    }

    public void Awake()
    {
        Transform buttonObject = transform.Find("Text");
        if (buttonObject != null)
        {
            buttonText = buttonObject.GetComponent<Text>();
        }
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        ChangeTextColour(Color.black);
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        ChangeTextColour(Color.white);
    }

    private void ChangeTextColour(Color c)
    {
        if (buttonText != null)
        {
            buttonText.color = c;
        }
    }

    public virtual void OnDisable()
    {
        ChangeTextColour(Color.white);
    }
}
