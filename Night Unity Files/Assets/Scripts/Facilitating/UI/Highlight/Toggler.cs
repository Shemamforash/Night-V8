using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Toggler : Highlight
{
    public string alternateTooltipText;
    private string currentTooltipText;

    public override void Awake()
    {
        base.Awake();
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
        if (childTexts[0].text.ToLower() == "on")
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
        childTexts[0].text = "ON";
        currentTooltipText = tooltipText;
    }

    protected virtual void Off()
    {
        childTexts[0].text = "OFF";
        currentTooltipText = alternateTooltipText;
    }
}
