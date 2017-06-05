using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Text buttonText;

    void Awake()
    {
        buttonText = transform.Find("Text").GetComponent<Text>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        Highlight();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        UnHighlight();
    }

    private void Highlight()
    {
        buttonText.color = Color.black;
    }

    private void UnHighlight()
    {
        buttonText.color = Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnHighlight();
    }

    public void OnDisable()
    {
        buttonText.color = Color.white;
    }
}
