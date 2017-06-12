using UnityEngine.EventSystems;
using UnityEngine;

namespace UI.Misc
{
    public class RefocusKeyboardInput : MonoBehaviour
    {
        private GameObject selected;

        public void Start()
        {
            selected = EventSystem.current.currentSelectedGameObject;
        }

        public void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(selected);
            }
            selected = EventSystem.current.currentSelectedGameObject;
        }
    }
}
