using UnityEngine.UI;
using UnityEngine;

namespace UI.Misc
{
    using UI.Highlight;
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
            activeButton.GetComponent<BorderHighlight>().BorderOn();
        }

        private void RevertActiveButton()
        {
            if (activeButton != null)
            {
                activeButton.GetComponent<BorderHighlight>().BorderOff();
                activeButton = null;
            }
        }

        public void OnDisable()
        {
            RevertActiveButton();
        }
    }
}