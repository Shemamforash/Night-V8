using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly List<HoldAction> _onHoldActions = new List<HoldAction>();
        private Button _button;
        public GameObject Border;
        [Range(0f, 5f)] public float FadeDuration = 0.5f;
        private bool _justEntered;
        private Action _onDownAction, _onUpAction;
        private Coroutine _fadeCoroutine;
        private readonly List<Image> _borderImages = new List<Image>();
        private bool _needsFadeIn;

        private class HoldAction
        {
            private readonly Action _holdAction;
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
                if (!(Time.time - _startTime > _duration)) return;
                _holdAction();
                Reset();
            }
        }

        public Button Button()
        {
            return _button == null ? GetComponent<Button>() : _button;
        }

        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            _button = GetComponent<Button>();
            if (Border == null) return;
            Border?.SetActive(false);
            _borderImages.AddRange(Helper.FindAllComponentsInChildren<Image>(Border?.transform));
        }

        public void Start()
        {
            if (_needsFadeIn) TryStartFade(1);
        }

        private void Enter()
        {
            UseSelectedColours();
            OnSelectActions?.Invoke();
            _justEntered = true;
        }

        private void Exit()
        {
            UseDeselectedColours();
            OnDeselectActions?.Invoke();
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            SetBorderColor(new Color(1, 1, 1, 0f));
        }

        public void AddOnClick(UnityAction a)
        {
            GetComponent<Button>().onClick.AddListener(a);
        }

        public void AddOnSelectEvent(Action a) => OnSelectActions += a;
        public void AddOnDeselectEvent(Action a) => OnDeselectActions += a;
        public void OnSelect(BaseEventData eventData) => Enter();
        public void OnDeselect(BaseEventData eventData) => Exit();
        public void OnPointerEnter(PointerEventData p) => Enter();
        public void OnPointerExit(PointerEventData p) => Exit();
        public void AddOnHold(Action a, float duration) => _onHoldActions.Add(new HoldAction(a, duration));

        private void UseSelectedColours()
        {
            Border?.SetActive(true);
            TryStartFade(1);
        }

        private void TryStartFade(int target)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            if (gameObject.activeInHierarchy) _fadeCoroutine = StartCoroutine(Fade(target));
            else _needsFadeIn = true;
        }

        private void SetBorderColor(Color c)
        {
            _borderImages.ForEach(b => b.color = c);
        }

        private IEnumerator Fade(float targetVal)
        {
            float alpha = 1 - targetVal;
            SetBorderColor(new Color(1, 1, 1, alpha));
            for (float t = 0; t < 1; t += Time.deltaTime / FadeDuration)
            {
                SetBorderColor(new Color(1, 1, 1, Mathf.Lerp(alpha, targetVal, t)));
                yield return null;
            }
        }

        private void UseDeselectedColours()
        {
            Border?.SetActive(false);
            TryStartFade(0);
        }

        private bool IsSelected()
        {
            return EventSystem.current.currentSelectedGameObject == gameObject;
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || !IsSelected()) return;
            if (axis == InputAxis.Fire)
            {
                if (isHeld)
                {
                    if (_button.gameObject == EventSystem.current.currentSelectedGameObject)
                    {
                        _onHoldActions.ForEach(a => a.ExecuteIfDone());
                    }
                }
                else
                {
                    _onHoldActions.ForEach(a => a.Reset());
                }
            }

            if (isHeld || _justEntered || axis != InputAxis.Vertical) return;
            if (direction > 0)
            {
                if (_button.navigation.selectOnUp == null && _onUpAction == null) return;
                _onUpAction?.Invoke();
                Exit();
            }
            else if (direction < 0)
            {
                if (_button.navigation.selectOnDown == null && _onDownAction == null) return;
                _onDownAction?.Invoke();
                Exit();
            }
        }

        private void OnDestroy()
        {
            InputHandler.UnregisterInputListener(this);
        }

        public void OnInputUp(InputAxis axis)
        {
            if (IsSelected() && axis == InputAxis.Vertical)
            {
                _justEntered = false;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void SetRightNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnRight = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnLeft = _button;
            target.SetNavigation(navigation);
        }

        public void SetLeftNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnLeft = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnRight = _button;
            target.SetNavigation(navigation);
        }

        public void SetUpNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnUp = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnDown = _button;
            target.SetNavigation(navigation);
        }

        public void SetDownNavigation(EnhancedButton target, bool reciprocate = true)
        {
            if (target == null) return;

            Navigation navigation = GetNavigation();
            navigation.selectOnDown = target.Button();
            SetNavigation(navigation);

            if (!reciprocate) return;
            navigation = target.GetNavigation();
            navigation.selectOnUp = _button;
            target.SetNavigation(navigation);
        }

        private Navigation GetNavigation()
        {
            return Button().navigation;
        }

        private void SetNavigation(Navigation navigation)
        {
            Button().navigation = navigation;
        }

        public void ClearNavigation()
        {
            Navigation navigation = _button.navigation;
            navigation.selectOnUp = null;
            navigation.selectOnDown = null;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;
            _button.navigation = navigation;
        }

        public void SetOnDownAction(Action a)
        {
            _onDownAction = a;
        }

        public void SetOnUpAction(Action a)
        {
            _onUpAction = a;
        }
    }
}