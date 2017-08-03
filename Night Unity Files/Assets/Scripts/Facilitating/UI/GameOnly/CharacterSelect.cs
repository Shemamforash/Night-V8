using UnityEngine.UI;
using UnityEngine;
using Characters;

namespace UI.GameOnly
{
    using UI.Highlight;
    using Menus;
    public class CharacterSelect : MonoBehaviour
    {
        private Selectable actionSelectable, selectedCharacter;
        private bool actionBarSelected = false;
        private Transform actionContainer;
        private InputListener inputListener = new InputListener();

        public void ExitCharacter()
        {
            if (selectedCharacter != null)
            {
                SetDetailedViewActive(false, selectedCharacter.transform.parent);
                selectedCharacter.Select();
                selectedCharacter = null;
            }
        }

        public void Start()
        {
            inputListener.OnCancel(ExitCharacter);
        }

        private void SetDetailedViewActive(bool active, Transform characterUIObject)
        {
            characterUIObject.Find("Detailed").gameObject.SetActive(active);
            characterUIObject.Find("Simple").gameObject.SetActive(!active);
            if (active)
            {
                CharacterManager.ExpandCharacter(characterUIObject.gameObject);
            }
            else
            {
                CharacterManager.CollapseCharacter(characterUIObject.gameObject);
            }
        }

        public void SelectCharacter(Selectable s)
        {
            if (selectedCharacter != null)
            {
                ExitCharacter();
            }
            selectedCharacter = s;
            SetDetailedViewActive(true, s.transform.parent);
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