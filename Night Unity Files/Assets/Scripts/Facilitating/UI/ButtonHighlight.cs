using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : Highlight, IPointerEnterHandler, IPointerExitHandler
{
    private Text buttonText;
    public bool hasBorder;

    public void Awake()
    {
        buttonText = transform.Find("Text").GetComponent<Text>();
    }

    public void SetBorderActive(bool active)
    {
        if (!initialised)
        {
            Initialise();
        }
        borderObject.SetActive(active);
    }

    public override void OnSelect(BaseEventData eventData)
    {
        Highlight();
    }

    public override void OnDeselect(BaseEventData eventData)
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
