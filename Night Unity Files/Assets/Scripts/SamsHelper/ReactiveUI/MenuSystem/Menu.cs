using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public abstract class Menu : MonoBehaviour
    {
        public Selectable DefaultSelectable;

        [HideInInspector] public bool PauseOnOpen = true;

        public bool PreserveLastSelected = true;
    }
}