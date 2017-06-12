using UnityEngine.UI;
using UnityEngine;
namespace UI.GameOnly
{
    using UI.Highlight;
    using Menus;
    public class CharacterSelect : BorderHighlight
    {
        private Selectable actionSelectable, selectedCharacter;
        private bool actionBarSelected = false;
        private Transform actionContainer;
        private InputListener inputListener = new InputListener();

        public void ExitCharacter()
        {
            if (selectedCharacter != null)
            {
                if (actionBarSelected)
                {
                    actionSelectable.Select();
                    actionContainer.gameObject.SetActive(false);
                    actionBarSelected = false;
                }
                else
                {
                    selectedCharacter.Select();
                    selectedCharacter = null;
                }
            }
        }

        public void Start()
        {
            inputListener.OnCancel(ExitCharacter);
            actionContainer = GameObject.Find("Actions").transform.Find("Viewport").Find("Content");
            actionContainer.gameObject.SetActive(false);
        }

        public void SelectCharacter(Selectable s)
        {
            selectedCharacter = s;
            s.transform.Find("Name Container").GetComponent<Selectable>().Select();
        }

        public void SelectActions(Selectable s)
        {
            actionSelectable = s;
            actionBarSelected = true;
            actionContainer.gameObject.SetActive(true);
            actionContainer.GetChild(0).GetComponent<Selectable>().Select();
        }
    }
}