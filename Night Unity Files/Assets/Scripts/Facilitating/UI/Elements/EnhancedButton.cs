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
    [RequireComponent(typeof(Button))]
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public List<Action> OnSelectActions = new List<Action>();
        private List<EnhancedText> _textChildren = new List<EnhancedText>();
        private Button _button;
        public bool UseBorder;

        public void Awake()
        {
            _button = GetComponent<Button>();
            _textChildren = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
        }
        
        public void AddOnSelectEvent(Action a)
        {
            OnSelectActions.Add(a);
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
            OnSelectActions.ForEach(a => a());
            ChangeTextColor(UiAppearanceController.Instance.SecondaryColor);
            _button.image.color = UiAppearanceController.Instance.MainColor;
        }

        private void UseDeselectedColours()
        {
            ChangeTextColor(UiAppearanceController.Instance.MainColor);
            _button.image.color = UiAppearanceController.Instance.BackgroundColor;
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