﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.Elements
{
    [RequireComponent(typeof(Button))]
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IInputListener
    {
        private readonly List<HoldAction> _onHoldActions = new List<HoldAction>();
        private Button _button;
        private bool _justEntered;
        private Action _onDownAction, _onUpAction;
        private static GameObject _borderPrefab;
        [SerializeField] private bool _hideBorder;
        private UIBorderController _border;
        private event Action OnSelectActions;
        private event Action OnDeselectActions;
        private bool _isSelected;
        private bool _enabled = true;
        private static EnhancedButton _currentButton;

        public void OnDeselect(BaseEventData eventData)
        {
//            Exit();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || !IsSelected()) return;
            if (axis == InputAxis.Fire)
                if (isHeld)
                {
                    if (_button.gameObject == EventSystem.current.currentSelectedGameObject) _onHoldActions.ForEach(a => a.ExecuteIfDone());
                }
                else
                {
                    _onHoldActions.ForEach(a => a.Reset());
                }

            if (isHeld || _justEntered || axis != InputAxis.Vertical) return;
            if (direction > 0)
            {
                if (_button.navigation.selectOnUp == null || _onUpAction == null) return;
                _onUpAction?.Invoke();
//                Exit();
            }
            else if (direction < 0)
            {
                if (_button.navigation.selectOnDown == null || _onDownAction == null) return;
                _onDownAction?.Invoke();
//                Exit();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Vertical && IsSelected()) _justEntered = false;
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void OnPointerEnter(PointerEventData p)
        {
            Enter();
        }

        public void OnPointerExit(PointerEventData p)
        {
//            Exit();
        }

        public void OnSelect(BaseEventData eventData)
        {
            Enter();
        }

        public Button Button()
        {
            if (_button == null) _button = GetComponent<Button>();
            return _button;
        }

        public void Awake()
        {
            InputHandler.RegisterInputListener(this);
            _button = GetComponent<Button>();
            if (_borderPrefab == null) _borderPrefab = Resources.Load<GameObject>("Prefabs/Borders/Border");
            if (_hideBorder) return;
            _border = Instantiate(_borderPrefab).GetComponent<UIBorderController>();
            _border.SetButton(this);
        }

        private void Enter()
        {
            if(_currentButton != null) _currentButton.Exit();
            _currentButton = this;
            OnSelectActions?.Invoke();
            _justEntered = true;
            if (!_hideBorder) _border.SetSelected();
            _isSelected = true;
            ButtonClickListener.Click(!_enabled);
        }

        private void Exit()
        {
            OnDeselectActions?.Invoke();
            _isSelected = false;
            if (_hideBorder) return;
            if (_enabled) _border.SetActive();
            else _border.SetDisabled();
        }

        public void AddOnClick(UnityAction a)
        {
            GetComponent<Button>().onClick.AddListener(a);
        }

        public void AddOnSelectEvent(Action a)
        {
            OnSelectActions += a;
        }

        public void AddOnDeselectEvent(Action a)
        {
            OnDeselectActions += a;
        }

        public void AddOnHold(Action a, float duration)
        {
            _onHoldActions.Add(new HoldAction(a, duration));
        }

        private bool IsSelected()
        {
//            return EventSystem.current.currentSelectedGameObject == gameObject;
            return _isSelected;
        }

        private void OnDestroy()
        {
            InputHandler.UnregisterInputListener(this);
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
            Navigation navigation = GetNavigation();
            navigation.selectOnDown = target == null ? null : target.Button();
            SetNavigation(navigation);
            if (target == null || !reciprocate) return;
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

        private class HoldAction
        {
            private readonly float _duration;
            private readonly Action _holdAction;
            private float _startTime;

            public HoldAction(Action a, float duration)
            {
                _holdAction = a;
                _duration = duration;
                Reset();
            }

            public void Reset()
            {
                _startTime = Time.realtimeSinceStartup;
            }

            public void ExecuteIfDone()
            {
                if (!(Time.realtimeSinceStartup - _startTime > _duration)) return;
                _holdAction();
                Reset();
            }
        }

        public void Select()
        {
            Button().Select();
        }

        public void SetEnabled(bool b)
        {
            if (b == _enabled) return;
            _enabled = b;
            if (!_enabled) _border.SetDisabled();
            else if (_isSelected) _border.SetSelected();
            else _border.SetActive();
        }

        public bool IsEnabled()
        {
            return _enabled;
        }
    }
}