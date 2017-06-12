using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Misc
{
    using UI.Highlight;
    public class TooltipController : MonoBehaviour
    {
        private RectTransform parentRect;
        private Highlight lastElement = null, currentElement = null;
        private Vector3[] corners = new Vector3[4];
        public GameObject tooltipObject;
        private Text tooltipText;

        public void Start()
        {
            tooltipText = tooltipObject.GetComponent<Text>();
            parentRect = tooltipObject.transform.parent.GetComponent<RectTransform>();
        }

        public void Update()
        {
            if (Input.GetAxis("Tooltip") != 0)
            {
                currentElement = EventSystem.current.currentSelectedGameObject.GetComponent<Highlight>();
                bool alreadyHighlighted = currentElement == lastElement && currentElement != null;
                if (!alreadyHighlighted)
                {
                    if (currentElement != null && currentElement.GetTooltip() != "")
                    {
                        tooltipObject.transform.parent.gameObject.SetActive(true);
                        tooltipText.text = currentElement.GetTooltip();
                        RectTransform elementTransform = currentElement.GetComponent<RectTransform>();
                        elementTransform.GetWorldCorners(corners);
                        Vector2 targetPosition = corners[0];
                        targetPosition.y -= 5;
                        tooltipObject.transform.parent.transform.position = targetPosition;
                    }
                    else
                    {
                        tooltipObject.transform.parent.gameObject.SetActive(false);
                    }
                }
                lastElement = currentElement;
            }
            else if (lastElement != null)
            {
                tooltipObject.transform.parent.gameObject.SetActive(false);
                lastElement = null;
            }
        }
    }
}