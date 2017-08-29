using System;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using SamsHelper;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Misc.Elements
{
    public class EnhancedButton : Button, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private List<Action> _onSelectActions = new List<Action>();
        private List<EnhancedText> _textChildren = new List<EnhancedText>();
        private Image _image;
        public bool UseBorder;

        public void Awake()
        {
            _textChildren = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
            _image = GetComponent<Image>();
        }
        
        public void AddOnSelectEvent(Action a)
        {
            _onSelectActions.Add(a);
        }

        public void OnSelect(BaseEventData eventData)
        {
            UseSelectedColours();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            UseDeselectedColours();
        }

        public void OnPointerEnter(PointerEventData p)
        {
            UseSelectedColours();
        }

        public void OnPointerExit(PointerEventData p)
        {
            UseDeselectedColours();
        }

        private void UseSelectedColours()
        {
            _onSelectActions.ForEach(a => a());
            ChangeTextColor(UiAppearanceController.Instance.SecondaryColor);
            _image.color = UiAppearanceController.Instance.MainColor;
        }

        private void UseDeselectedColours()
        {
            ChangeTextColor(UiAppearanceController.Instance.MainColor);
            _image.color = UiAppearanceController.Instance.SecondaryColor;
        }

        private void ChangeTextColor(Color c)
        {
            foreach (EnhancedText text in _textChildren)
            {
                text.SetColor(c);
            }
        }
    }
}