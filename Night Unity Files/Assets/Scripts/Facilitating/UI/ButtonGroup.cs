using UnityEngine.UI;
using UnityEngine;

namespace UI.Misc
{
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
        }

        private void RevertActiveButton()
        {
            if (activeButton != null)
            {
                activeButton = null;
            }
        }

        public void OnDisable()
        {
            RevertActiveButton();
        }
    }
}