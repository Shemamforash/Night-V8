using TMPro;
using UI.Highlight;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Facilitating.UI
{
    public class TooltipController : MonoBehaviour
    {
        private Highlight _lastElement, _currentElement;
        private readonly Vector3[] _corners = new Vector3[4];
        public GameObject TooltipObject;
        private TextMeshProUGUI _tooltipText;

        public void Start()
        {
            _tooltipText = TooltipObject.GetComponent<TextMeshProUGUI>();
        }

        public void Update()
        {
            if (Input.GetAxis("Tooltip") != 0)
            {
                _currentElement = EventSystem.current.currentSelectedGameObject.GetComponent<Highlight>();
                bool alreadyHighlighted = _currentElement == _lastElement && _currentElement != null;
                if (!alreadyHighlighted)
                {
                    if (_currentElement != null && _currentElement.GetTooltip() != "")
                    {
                        TooltipObject.transform.parent.gameObject.SetActive(true);
                        _tooltipText.text = _currentElement.GetTooltip();
                        RectTransform elementTransform = _currentElement.GetComponent<RectTransform>();
                        elementTransform.GetWorldCorners(_corners);
                        Vector2 targetPosition = _corners[0];
                        targetPosition.y -= 5;
                        TooltipObject.transform.parent.transform.position = targetPosition;
                    }
                    else
                    {
                        TooltipObject.transform.parent.gameObject.SetActive(false);
                    }
                }
                _lastElement = _currentElement;
            }
            else if (_lastElement != null)
            {
                TooltipObject.transform.parent.gameObject.SetActive(false);
                _lastElement = null;
            }
        }
    }
}