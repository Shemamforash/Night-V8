using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class Menu : MonoBehaviour
    {
        public Selectable DefaultSelectable;
        public bool PauseOnOpen = true;
        public bool PreserveLastSelected = true;
    }
}