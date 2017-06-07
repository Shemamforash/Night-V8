using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Toggler : Highlight
{
    public string alternateTooltipText;
    private string currentTooltipText;

    public virtual void Start()
    {
        currentTooltipText = tooltipText;
        if(alternateTooltipText == ""){
            alternateTooltipText = currentTooltipText;
        }
        On();
    }

    public override string GetTooltip()
    {
        return currentTooltipText;
    }

    public void Toggle()
    {
        if (buttonText.text.ToLower() == "on")
        {
            Off();
        }
        else
        {
            On();
        }
    }

    protected virtual void On()
    {
        buttonText.text = "ON";
        currentTooltipText = tooltipText;
    }

    protected virtual void Off()
    {
        buttonText.text = "OFF";
        currentTooltipText = alternateTooltipText;
    }
}
