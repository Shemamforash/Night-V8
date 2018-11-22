using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Facilitating.UIControllers.Inventories
{
    public class InventoryTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
    {
        private UiInventoryMenuController _menu;
        private InventoryTab _prevTab;
        private InventoryTab _nextTab;
        private Image _highlightImage;
        private static InventoryTab _currentTab;

        public void SetMenu(UiInventoryMenuController menu)
        {
            _highlightImage = gameObject.GetComponent<Image>();
            _highlightImage.GetComponent<Button>().onClick.AddListener(Select);
            _menu = menu;
        }

        public static InventoryTab CurrentTab()
        {
            return _currentTab;
        }

        public void Hover()
        {
            _highlightImage.DOFade(0.5f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public void Select()
        {
            if (_currentTab == null)
            {
                if (_prevTab == null) UiGearMenuController.LeftTab().InstantFade();
                if (_nextTab == null) UiGearMenuController.RightTab().InstantFade();
            }
            else
            {
                UiGearMenuController.PlayTabAudio();
                if (_currentTab == _nextTab)
                {
                    if (_prevTab == null) UiGearMenuController.LeftTab().FlashAndFade();
                    else UiGearMenuController.LeftTab().Flash();
                    UiGearMenuController.RightTab().FadeIn();
                }
                else if (_currentTab == _prevTab)
                {
                    if (_nextTab == null) UiGearMenuController.RightTab().FlashAndFade();
                    else UiGearMenuController.RightTab().Flash();
                    UiGearMenuController.LeftTab().FadeIn();
                }

                _currentTab.Deselect();
            }

            _currentTab = this;
            _highlightImage.DOFade(1f, 0.5f).SetUpdate(UpdateType.Normal, true);
            UiGearMenuController.OpenInventoryMenu(_menu);
        }

        public void Deselect()
        {
            _highlightImage.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public void SetNeighbors(InventoryTab prevTab, InventoryTab nextTab)
        {
            _prevTab = prevTab;
            _nextTab = nextTab;
        }

        public void SelectNextTab()
        {
            if (_nextTab == null) return;
            _nextTab.Select();
        }

        public void SelectPreviousTab()
        {
            if (_prevTab == null) return;
            _prevTab.Select();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (this == _currentTab) return;
            Hover();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (this == _currentTab) return;
            Deselect();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Select();
        }

        public static void ClearActiveTab()
        {
            if (_currentTab == null) return;
            _currentTab.Deselect();
            _currentTab = null;
        }
    }
}