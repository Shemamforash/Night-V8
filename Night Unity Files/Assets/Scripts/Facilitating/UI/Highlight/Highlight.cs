using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Highlight
{
    public class Highlight : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        /*Base class for all interactive objects.
        If it's interactive, it must have it's colours inverted when selected, and must have a tooltip.
        */

        protected Text[] childTexts;
        public string tooltipText;

        public virtual string GetTooltip()
        {
            return tooltipText;
        }

        public virtual void Awake()
        {
            childTexts = gameObject.GetComponentsInChildren<Text>();
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
            foreach (Text buttonText in childTexts)
            {
                buttonText.color = c;
            }
        }

        public virtual void OnDisable()
        {
            ChangeTextColour(Color.white);
        }
    }
}