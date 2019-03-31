using System;
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
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IInputListener
    {
        private readonly List<HoldAction> _onHoldActions = new List<HoldAction>();
        private Button _button;
        private static GameObject _borderPrefab;
        [SerializeField] private bool _hideBorder;
        private UIBorderController _border;
        private event Action OnSelectActions;
        private event Action OnDeselectActions;
        private bool _isSelected;
        private bool _enabled = true;
        private static EnhancedButton _currentButton;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_button == null || !IsSelected()) return;
            if (axis != InputAxis.Accept) return;
            if (!isHeld)
            {
                _onHoldActions.ForEach(a => a.Reset());
                return;
            }

            if (_button.gameObject == EventSystem.current.currentSelectedGameObject) _onHoldActions.ForEach(a => a.ExecuteIfDone());
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
            if (_currentButton != null) _currentButton.Exit();
            _currentButton = this;
            OnSelectActions?.Invoke();
            if (!_hideBorder) _border.SetSelected();
            _isSelected = true;
            ButtonClickListener.Click(!_enabled);
        }

        private void Exit()
        {
            OnDeselectActions?.Invoke();
            _isSelected = false;
            if (_hideBorder) return;
//            if (_enabled) _border.SetActive();
            else _border.SetDisabled();
        }

        private void OnDestroy()
        {
            InputHandler.UnregisterInputListener(this);
        }

        public static void DeselectCurrent()
        {
            _currentButton.Exit();
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

        public void Select() => Button().Select();

        public bool IsEnabled() => _enabled;

        public void AddOnClick(UnityAction a) => GetComponent<Button>().onClick.AddListener(a);

        public void AddOnSelectEvent(Action a) => OnSelectActions += a;

        public void AddOnDeselectEvent(Action a) => OnDeselectActions += a;

        public void AddOnHold(Action a, float duration) => _onHoldActions.Add(new HoldAction(a, duration));

        private bool IsSelected() => _isSelected;

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void OnPointerEnter(PointerEventData p) => Select();

        public void OnSelect(BaseEventData eventData) => Enter();
    }
}