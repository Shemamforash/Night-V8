using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour
    {
        public Selectable DefaultSelectable;

        [HideInInspector] public bool PauseOnOpen = true;

        public bool PreserveLastSelected = true;
        private CanvasGroup _canvasGroup;

        public virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if(_canvasGroup == null) Debug.Log(gameObject.name);
        }

        public virtual void Enter()
        {
            _canvasGroup.alpha = 1;
        }

        public void Exit()
        {
            _canvasGroup.alpha = 0;
        }
    }
}