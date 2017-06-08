using UnityEngine.UI;
using UnityEngine;

public class CharacterSelect : BorderHighlight, InputListener
{
    private Selectable firstSelectInCharacter, outerCharacterSelectable, actionSelectable;
    private bool characterIsSelected = false, actionBarSelected = false;
    private Transform actionContainer;

    public void OnCancel()
    {
        if (characterIsSelected)
        {
            if (actionBarSelected)
            {
                actionSelectable.Select();
                actionContainer.gameObject.SetActive(false);
				actionBarSelected = false;
            }
            else
            {
                outerCharacterSelectable.Select();
                characterIsSelected = false;
            }
        }
    }

    public void OnConfirm()
    {

    }

    public void Start()
    {
        InputSpeaker.RegisterForInput(this);
        outerCharacterSelectable = GetComponent<Selectable>();
        firstSelectInCharacter = transform.Find("Name Container").GetComponent<Selectable>();
        actionContainer = GameObject.Find("Actions").transform.Find("Viewport").Find("Content");
        actionContainer.gameObject.SetActive(false);
    }

    public void SelectCharacter()
    {
        firstSelectInCharacter.Select();
        characterIsSelected = true;
    }

    public void SelectActions(Selectable s)
    {
		actionSelectable = s;
        actionBarSelected = true;
        actionContainer.gameObject.SetActive(true);
        actionContainer.GetChild(0).GetComponent<Selectable>().Select();
    }
}
