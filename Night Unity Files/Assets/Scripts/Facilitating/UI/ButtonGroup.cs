using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class ButtonGroup : MonoBehaviour
{
    public Button initialActiveButton;
    private Button activeButton;

    public void OnEnable()
    {
        if (activeButton != initialActiveButton)
        {
            MakeActiveInGroup(initialActiveButton);
        }
    }

    public void MakeActiveInGroup(Button btn)
    {
        RevertActiveButton();
        activeButton = btn;
        activeButton.GetComponent<ButtonHighlight>().SetBorderActive(true);
    }

    private void RevertActiveButton()
    {
        if (activeButton != null)
        {
            activeButton.GetComponent<ButtonHighlight>().SetBorderActive(false);
			activeButton = null;
        }
    }

    public void OnDisable()
    {
        RevertActiveButton();
    }
}
