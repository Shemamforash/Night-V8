using DG.Tweening;
using Game.Global;
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
        private bool _active;

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

        private void Hover()
        {
            _highlightImage.DOFade(0.5f, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        private bool TabIsNext()
        {
            InventoryTab next = _nextTab;
            while (next != null)
            {
                if (_currentTab == next) return true;
                next = next._nextTab;
            }

            return false;
        }

        private bool TabIsPrevious()
        {
            InventoryTab prev = _prevTab;
            while (prev != null)
            {
                if (_currentTab == prev) return true;
                prev = prev._prevTab;
            }

            return false;
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
                UiGearMenuController.PlayAudio(AudioClips.TabChange);
                if (TabIsNext())
                {
                    if (_prevTab == null) UiGearMenuController.LeftTab().FlashAndFade();
                    else UiGearMenuController.LeftTab().Flash();
                    UiGearMenuController.RightTab().FadeIn();
                }
                else if (TabIsPrevious())
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

        private void Deselect()
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
            if (TutorialManager.Instance.IsTutorialVisible()) return;
            Select();
        }

        public static void ClearActiveTab()
        {
            if (_currentTab == null) return;
            _currentTab.Deselect();
            _currentTab = null;
        }

        public UiInventoryMenuController GetMenu()
        {
            return _menu;
        }

        public void UpdateActive()
        {
            bool unlocked = _menu.Unlocked();
            _active = unlocked;
            transform.parent.gameObject.SetActive(unlocked);
        }

        public bool Active()
        {
            return _active;
        }
    }
}