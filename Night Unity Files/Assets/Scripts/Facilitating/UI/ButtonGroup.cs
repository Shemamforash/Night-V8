using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI
{
    public class ButtonGroup : MonoBehaviour
    {
        private Button activeButton;
        public Button initialActiveButton;

        public void OnEnable()
        {
            if (activeButton != initialActiveButton) MakeActiveInGroup(initialActiveButton);
        }

        public void MakeActiveInGroup(Button btn)
        {
            RevertActiveButton();
            activeButton = btn;
        }

        private void RevertActiveButton()
        {
            activeButton = null;
        }

        public void OnDisable()
        {
            RevertActiveButton();
        }
    }
}