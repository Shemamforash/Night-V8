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
    public class EnhancedButton : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        private Button _button;
        private static GameObject _borderPrefab;
        [SerializeField] private bool _hideBorder;
        private UIBorderController _border;
        private event Action OnSelectActions;
        private event Action OnDeselectActions;
        private bool _isSelected;
        private bool _enabled = true;
        private static EnhancedButton _currentButton;


        public Button Button()
        {
            if (_button == null) _button = GetComponent<Button>();
            return _button;
        }

        public void Awake()
        {
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

        public static void DeselectCurrent()
        {
            if (_currentButton == null) return;
            _currentButton.Exit();
        }

        public void Select() => Button().Select();

        public bool IsEnabled() => _enabled;

        public void AddOnClick(UnityAction a) => GetComponent<Button>().onClick.AddListener(a);

        public void AddOnSelectEvent(Action a) => OnSelectActions += a;

        public void AddOnDeselectEvent(Action a) => OnDeselectActions += a;

        private bool IsSelected() => _isSelected;

        public void OnPointerEnter(PointerEventData p) => Select();

        public void OnSelect(BaseEventData eventData) => Enter();
    }
}