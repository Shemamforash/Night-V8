using UnityEngine.UI;
using UnityEngine;

public class ButtonGroup : MonoBehaviour
{
    private Button activeButton = null;
    private Sprite buttonBorderImage;

    public void Start()
    {
        buttonBorderImage = Resources.Load("ButtonBorder", typeof(Sprite)) as Sprite;
    }

    public void MakeActiveInGroup(Button btn)
    {
		RevertActiveButton();
        activeButton = btn;
        activeButton.enabled = false;
        activeButton.transform.Find("Text").GetComponent<Text>().color = Color.white;
        activeButton.GetComponent<Button>().image.sprite = buttonBorderImage;
    }

    private void RevertActiveButton()
    {
        if (activeButton != null)
        {
            activeButton.enabled = true;
            activeButton.GetComponent<Button>().image.sprite = null;
        }
    }

    public void OnDisable()
    {
        RevertActiveButton();
    }
}
