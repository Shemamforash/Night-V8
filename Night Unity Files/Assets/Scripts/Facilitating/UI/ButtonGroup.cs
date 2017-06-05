using UnityEngine.UI;
using UnityEngine;

public class ButtonGroup : MonoBehaviour
{
    private Button activeButton = null;
    private Sprite buttonBorderImage;

    public void Start()
    {
        buttonBorderImage = Resources.Load("Images/ButtonBorder", typeof(Sprite)) as Sprite;
    }

    public void MakeActiveInGroup(Button btn)
    {
        RevertActiveButton();
        activeButton = btn;
        activeButton.GetComponent<ButtonHighlight>().enabled = false;
        ColorBlock colors = activeButton.GetComponent<Button>().colors;
        colors.normalColor = Color.white;
        activeButton.GetComponent<Button>().colors = colors;
        activeButton.transform.Find("Text").GetComponent<Text>().color = Color.white;
        activeButton.GetComponent<Button>().image.sprite = buttonBorderImage;
    }

    private void RevertActiveButton()
    {
        if (activeButton != null)
        {
            activeButton.GetComponent<ButtonHighlight>().enabled = true;
            activeButton.GetComponent<Button>().image.sprite = null;
            ColorBlock colors = activeButton.GetComponent<Button>().colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            activeButton.GetComponent<Button>().colors = colors;
        }
    }

    public void OnDisable()
    {
        RevertActiveButton();
    }
}
