using UnityEngine.UI;
using UnityEngine;
using Characters;

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
                    SetDetailedViewActive(false, selectedCharacter.transform);
                    selectedCharacter.Select();
                    selectedCharacter = null;
                }
            }
        }

        public void Start()
        {
            inputListener.OnCancel(ExitCharacter);
        }

        private void SetDetailedViewActive(bool active, Transform characterUIObject)
        {
            if (active)
            {
                CharacterManager.ExpandCharacter(characterUIObject.gameObject);
            }
            else
            {
                CharacterManager.CollapseCharacter(characterUIObject.gameObject);
            }
            for (int i = 0; i < characterUIObject.childCount; ++i)
            {
                Transform t = characterUIObject.GetChild(i);
                t.Find("Detailed").gameObject.SetActive(active);
            }
        }

        public void SelectCharacter(Selectable s)
        {
            selectedCharacter = s;
            SetDetailedViewActive(true, s.transform);
            // s.transform.Find("Name Container").GetComponent<Selectable>().Select();
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