using UnityEngine.EventSystems;

public class ManualSlider : BorderHighlight
{
    public override void OnSelect(BaseEventData eventData)
    {
        BorderOn();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        BorderOff();
    }
}
