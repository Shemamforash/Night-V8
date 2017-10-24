using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputHandler : MonoBehaviour
    {
        private readonly Dictionary<InputAxis, InputPress> _inputPressList = new Dictionary<InputAxis, InputPress>();
        private static float _lastInputValue;
        private static InputHandler _instance;
        private static List<InputPress> _pressedKeys = new List<InputPress>();

        public void Awake()
        {
            _inputPressList[InputAxis.Submit] = new InputPress("Submit");
            _inputPressList[InputAxis.Cancel] = new InputPress("Cancel");
            _inputPressList[InputAxis.Fire] = new InputPress("Fire");
            _inputPressList[InputAxis.Reload] = new InputPress("Reload");
            _inputPressList[InputAxis.Vertical] = new InputPress("Vertical");
            _inputPressList[InputAxis.Horizontal] = new InputPress("Horizontal");
            _instance = this;
        }

        public static InputHandler Instance()
        {
            return _instance ?? FindObjectOfType<InputHandler>();
        }

        public void Update()
        {
            foreach (InputPress i in _inputPressList.Values)
            {
                i.CheckPress();
            }
        }

        //Add Events
        public void AddOnPressEvent(InputAxis axis, Action a) => _inputPressList[axis].OnPressAny += a;
        public void AddOnPositivePressEvent(InputAxis axis, Action a) => _inputPressList[axis].OnPressPositive += a;
        public void AddOnNegativePressEvent(InputAxis axis, Action a) => _inputPressList[axis].OnPressNegative += a;
        public void AddOnHoldEvent(InputAxis axis, Action a) => _inputPressList[axis].OnHoldAny += a;
        public void AddOnPositiveHoldEvent(InputAxis axis, Action a) => _inputPressList[axis].OnHoldPositive += a;
        public void AddOnNegativeHoldEvent(InputAxis axis, Action a) => _inputPressList[axis].OnHoldNegative += a;
        public void AddOnReleaseEvent(InputAxis axis, Action a) => _inputPressList[axis].OnRelease += a;

        //Remove Events
        public void RemoveOnPressEvent(InputAxis axis, Action a) => _inputPressList[axis].OnPressAny -= a;
        public void RemoveOnPositivePressEvent(InputAxis axis, Action a) => _inputPressList[axis].OnPressPositive -= a;
        public void RemoveOnNegativePressEvent(InputAxis axis, Action a) => _inputPressList[axis].OnPressNegative -= a;
        public void RemoveOnHoldEvent(InputAxis axis, Action a) => _inputPressList[axis].OnHoldAny -= a;
        public void RemoveOnPositiveHoldEvent(InputAxis axis, Action a) => _inputPressList[axis].OnHoldPositive -= a;
        public void RemoveOnNegativeHoldEvent(InputAxis axis, Action a) => _inputPressList[axis].OnHoldNegative -= a;
        public void RemoveOnReleaseEvent(InputAxis axis, Action a) => _inputPressList[axis].OnRelease -= a;

        private class InputPress
        {
            private bool _pressed;
            private readonly string _axisName;
            public event Action OnRelease;
            public event Action OnPressAny, OnHoldAny;
            public event Action OnPressPositive, OnHoldPositive;
            public event Action OnPressNegative, OnHoldNegative;


            public InputPress(string axisName)
            {
                _axisName = axisName;
            }

            public void CheckPress()
            {
                _lastInputValue = UnityEngine.Input.GetAxisRaw(_axisName);
                if (Math.Abs(_lastInputValue) > 0.0001f)
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        AddPressedKey(this);
                        BroadcastPress();
                    }
                    BroadcastHeld();
                }
                else
                {
                    _pressed = false;
                    RemovePressedKey(this);
                    BroadcastRelease();
                }
            }

            private void BroadcastPress()
            {
                if (_lastInputValue < 0)
                {
                    OnPressPositive?.Invoke();
                }
                else if (_lastInputValue > 0)
                {
                    OnPressNegative?.Invoke();
                }
                OnPressAny?.Invoke();
            }

            private void BroadcastRelease() => OnRelease?.Invoke();

            private void BroadcastHeld()
            {
                if (_lastInputValue < 0)
                {
                    OnHoldPositive?.Invoke();
                }
                else if (_lastInputValue > 0)
                {
                    OnHoldNegative?.Invoke();
                }
                OnHoldAny?.Invoke();
            }
        }

        public static float LastInputValue() => _lastInputValue;

        private static void AddPressedKey(InputPress key)
        {
            _pressedKeys.Add(key);
        }

        private static void RemovePressedKey(InputPress key)
        {
            _pressedKeys.Remove(key);
            if (_pressedKeys.Count == 0)
            {
                BroadcastNoPress();
            }
        }

        private static void BroadcastNoPress() => Instance().OnNoPress?.Invoke();
        public void AddOnNoPress(Action a) => OnNoPress += a;
        public event Action OnNoPress;
    }
}