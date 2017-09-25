using System;
using System.Collections;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using SamsHelper.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.Elements
{
    [RequireComponent(typeof(Button))]
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private event Action OnSelectActions;
        private readonly List<HoldAction> OnHoldActions = new List<HoldAction>();
        private List<EnhancedText> _textChildren = new List<EnhancedText>();
        private Button _button;
        public GameObject Border;
        [SerializeField]
        private bool _useBorder = true;
        public float FadeDuration = 0.5f;

        private class HoldAction
        {
            private Action _holdAction;
            private readonly float _duration;
            private float _startTime;

            public HoldAction(Action a, float duration)
            {
                _holdAction = a;
                _duration = duration;
                Reset();
            }
            
            public void Reset()
            {
                _startTime = Time.time;
            }

            public void ExecuteIfDone()
            {
                if (Time.time - _startTime > _duration)
                {
                    _holdAction();
                    Reset();
                }
            }
        }

        public void AddOnClick(UnityAction a)
        {
            _button.onClick.AddListener(a);
        }
        
        public void Awake()
        {
            _button = GetComponent<Button>();
            _textChildren = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
        }

        public void Start()
        {
            InputSpeaker.Instance().AddOnHoldEvent(InputAxis.Submit, OnHold);
            InputSpeaker.Instance().AddOnPressEvent(InputAxis.Submit, () => OnHoldActions.ForEach(a => a.Reset()));
        }

        public void AddOnSelectEvent(Action a)
        {
            OnSelectActions += a;
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

        public void DisableBorder()
        {
            _useBorder = false;
        }

        private void UseSelectedColours()
        {
            if (Border != null && _useBorder)
            {
                Border.SetActive(true);
                StartCoroutine(Fade(1));
            }
            else
            {
                if (OnSelectActions != null) OnSelectActions();
                ChangeTextColor(UiAppearanceController.Instance.SecondaryColor);
                if (_button.image != null)
                {
                    _button.image.color = UiAppearanceController.Instance.MainColor;
                }
            }
        }

        private IEnumerator Fade(float targetVal)
        {
            float alpha = 1 - targetVal;
            foreach (Transform child in Helper.FindAllChildren(Border.transform))
            {
                child.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            }
            for (float t = 0; t < 1; t += Time.deltaTime / FadeDuration)
            {
                Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, targetVal, t));
                foreach (Transform child in Helper.FindAllChildren(Border.transform))
                {
                    child.GetComponent<Image>().color = newColor;
                }
                yield return null;
            }
        }

        private void UseDeselectedColours()
        {
            if (Border != null && _useBorder)
            {
                Border.SetActive(false);
                StartCoroutine(Fade(0));
            }
            else
            {
                ChangeTextColor(UiAppearanceController.Instance.MainColor);
                if (_button.image != null)
                {
                    _button.image.color = UiAppearanceController.Instance.BackgroundColor;
                }
            }
        }

        private void ChangeTextColor(Color c)
        {
            foreach (EnhancedText text in _textChildren)
            {
                text.SetColor(c);
            }
        }

        public void AddOnHold(Action a, float duration)
        {
            OnHoldActions.Add(new HoldAction(a, duration));
        }

        public void OnHold()
        {
            if (_button == null)
            {
                InputSpeaker.Instance().RemoveOnHoldEvent(OnHold, InputAxis.Submit);
            }
            else if(_button.gameObject == EventSystem.current.currentSelectedGameObject) OnHoldActions.ForEach(a => a.ExecuteIfDone());
        }
    }
}