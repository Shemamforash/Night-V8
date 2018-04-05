using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using TMPro;

namespace UI.Highlight
{
    public class Highlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /*Base class for all interactive objects.
        If it's interactive, it must have it's colours inverted when selected, and must have a tooltip.
        */

        protected List<TextMeshProUGUI> childTexts = new List<TextMeshProUGUI>();

        public virtual void Awake()
        {
            List<Transform> children = Helper.FindAllChildren(transform);
            foreach (Transform t in children)
            {
                TextMeshProUGUI text = t.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    childTexts.Add(text);
                }
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            ChangeTextColour(Color.white);
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            Fade();
        }

        private void Fade()
        {
            ChangeTextColour(UiAppearanceController.FadedColour);
        }
        
        public void OnPointerEnter(PointerEventData p)
        {
            ChangeTextColour(Color.white);
        }

        public void OnPointerExit(PointerEventData p)
        {
            Fade();
        }

        private void ChangeTextColour(Color c)
        {
            foreach (TextMeshProUGUI buttonText in childTexts)
            {
                buttonText.color = c;
            }
        }

        public void OnDisable()
        {
            ChangeTextColour(Color.white);
        }
    }
}