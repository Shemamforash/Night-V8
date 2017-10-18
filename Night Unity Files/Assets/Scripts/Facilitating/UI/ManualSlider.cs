using UnityEngine.EventSystems;

namespace UI.Highlight
{
    public class ManualSlider : BorderHighlight
    {
        public override void Awake()
        {

        }

        public override void OnSelect(BaseEventData eventData)
        {
            BorderOn();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            BorderOff();
        }
    }
}
