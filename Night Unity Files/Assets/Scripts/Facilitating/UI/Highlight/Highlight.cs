using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SamsHelper;

namespace UI.Highlight
{
    public class Highlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /*Base class for all interactive objects.
        If it's interactive, it must have it's colours inverted when selected, and must have a tooltip.
        */

        protected List<Text> childTexts = new List<Text>();
        public string tooltipText;

        public virtual string GetTooltip()
        {
            return tooltipText;
        }

        public virtual void Awake()
        {
            List<Transform> children = Helper.FindAllChildren(transform);
            foreach (Transform t in children)
            {
                Text text = t.GetComponent<Text>();
                if (text != null)
                {
                    childTexts.Add(text);
                }
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

        public void OnPointerEnter(PointerEventData p)
        {
            ChangeTextColour(Color.black);
        }

        public void OnPointerExit(PointerEventData p)
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