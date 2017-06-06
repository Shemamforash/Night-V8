using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour
{
    public GameObject tooltipObject;
    private Text tooltipText;

    public void Start()
    {
        tooltipText = tooltipObject.GetComponent<Text>();
    }

    public void Update()
    {
        if (Input.GetAxis("Tooltip") != 0)
        {
            Highlight currentElement = EventSystem.current.currentSelectedGameObject.GetComponent<Highlight>();
            if (currentElement != null && currentElement.tooltipInfo != "")
            {
                tooltipObject.transform.parent.gameObject.SetActive(true);
				tooltipObject.transform.parent.transform.position = currentElement.transform.position;
                tooltipText.text = currentElement.tooltipInfo;
            }
            else
            {
                tooltipObject.transform.parent.gameObject.SetActive(false);
                tooltipObject.SetActive(false);
            }
        }
        else
        {
            tooltipObject.transform.parent.gameObject.SetActive(false);
        }
    }
}
