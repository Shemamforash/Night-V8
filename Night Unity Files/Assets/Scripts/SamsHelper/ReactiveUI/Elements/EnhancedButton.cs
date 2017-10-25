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
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IInputListener
    {
        private event Action OnSelectActions;
        private event Action OnDeselectActions;
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

        
        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            _button = GetComponent<Button>();
            _textChildren = Helper.FindAllComponentsInChildren<EnhancedText>(transform);
        }

        private void Enter()
        {
            UseSelectedColours();
            OnSelectActions?.Invoke();
        }

        private void Exit()
        {
            UseDeselectedColours();
            OnDeselectActions?.Invoke();
        }
        
        public void AddOnClick(UnityAction a) => _button.onClick.AddListener(a);
        public void AddOnSelectEvent(Action a) => OnSelectActions += a;
        public void AddOnDeselectEvent(Action a) => OnDeselectActions += a;
        public void OnSelect(BaseEventData eventData) => Enter();
        public void OnDeselect(BaseEventData eventData) => Exit();
        public void OnPointerEnter(PointerEventData p) => Enter();
        public void OnPointerExit(PointerEventData p) => Exit();
        public void AddOnHold(Action a, float duration) => OnHoldActions.Add(new HoldAction(a, duration));

        public void DisableBorder() => _useBorder = false;

        private void UseSelectedColours()
        {
            if (Border != null && _useBorder)
            {
                Border.SetActive(true);
                StartCoroutine(Fade(1));
            }
            else
            {
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

        public void ChangeTextColor(Color c)
        {
            foreach (EnhancedText text in _textChildren)
            {
                text.SetColor(c);
            }
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || axis != InputAxis.Submit) return;
            if (isHeld)
            {
                if (_button.gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    OnHoldActions.ForEach(a => a.ExecuteIfDone());
                }
            }
            else
            {
                OnHoldActions.ForEach(a => a.Reset());
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }
    }
}