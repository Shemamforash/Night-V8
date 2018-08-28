using DefaultNamespace;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers.Inventories
{
    public abstract class UiInventoryMenuController : MonoBehaviour
    {
        public void Awake()
        {
            CacheElements();
            Initialise();
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }

        protected abstract void CacheElements();
        protected abstract void Initialise();
        
        protected abstract class BasicListElement : ListElement
        {
            protected EnhancedText CentreText;
            protected EnhancedText LeftText;
            protected EnhancedText RightText;

            protected override void SetVisible(bool visible)
            {
                CentreText.gameObject.SetActive(visible);
                LeftText.gameObject.SetActive(visible);
                RightText.gameObject.SetActive(visible);
            }

            protected override void CacheUiElements(Transform transform)
            {
                CentreText = transform.gameObject.FindChildWithName<EnhancedText>("Name");
                LeftText = transform.gameObject.FindChildWithName<EnhancedText>("Type");
                RightText = transform.gameObject.FindChildWithName<EnhancedText>("Dps");
            }

            public override void SetColour(Color c)
            {
                CentreText.SetColor(c);
                LeftText.SetColor(c);
                RightText.SetColor(c);
            }
        }
    }
}