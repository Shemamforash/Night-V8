using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Text buttonText;
    public bool hasBorder;
    private GameObject borderObject;
    private bool initialised = false;
	public GameObject borderPrefab;

    public void Awake()
    {
        buttonText = transform.Find("Text").GetComponent<Text>();
		borderPrefab = Resources.Load("Prefabs/Border", typeof(GameObject)) as GameObject;
    }

    public void Initialise()
    {
        initialised = true;
        if (hasBorder)
        {
            AddBorder();
        }
    }

    private void AddBorder()
    {
        borderObject = Instantiate(borderPrefab);
        borderObject.name = "Border";
        borderObject.transform.SetParent(transform);
        RectTransform rect = borderObject.GetComponent<RectTransform>();
        rect.localScale = new Vector2(1, 1);
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, 0);
        borderObject.SetActive(false);
    }

    public void SetBorderActive(bool active)
    {
        if (!initialised)
        {
            Initialise();
        }
        borderObject.SetActive(active);
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
