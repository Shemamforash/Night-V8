using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputHandler : MonoBehaviour
    {
        private readonly Dictionary<InputAxis, InputPress> _inputPressList = new Dictionary<InputAxis, InputPress>();
        private static InputHandler _instance;
        private static readonly List<InputPress> _pressedKeys = new List<InputPress>();
        private event Action OnNoPress;
        private static readonly List<IInputListener> InputListeners = new List<IInputListener>();
        private const float _doubleTapDuration = 300;


        public void Awake()
        {
            foreach (InputAxis axis in Enum.GetValues(typeof(InputAxis)))
            {
                AddInputPress(axis);
            }
            _instance = this;
        }

        private void AddInputPress(InputAxis axis)
        {
            _inputPressList[axis] = new InputPress(axis);
        }

        private static InputHandler Instance()
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

        private class InputPress
        {
            private bool _pressed;
            private readonly InputAxis _axis;
            private float _lastInputValue;
            private long _timeAtLastPress;
            private float _directionAtLastPress;

            public InputPress(InputAxis axis)
            {
                _axis = axis;
            }

            private void CheckDoubleTap()
            {
                long currentTime = Helper.TimeInMillis();
                float timeBetweenClicks = currentTime - _timeAtLastPress;
                if (timeBetweenClicks < _doubleTapDuration && Helper.ValuesHaveSameSign(_directionAtLastPress, _lastInputValue))
                {
                    BroadCastDoubleTap(_axis, _lastInputValue);
                    currentTime = 0;
                }
                _directionAtLastPress = _lastInputValue;
                _timeAtLastPress = currentTime;
            }

            public void CheckPress()
            {
                _lastInputValue = UnityEngine.Input.GetAxisRaw(_axis.ToString());
                if (Math.Abs(_lastInputValue) > 0f)
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        AddPressedKey(this);
                        BroadcastInputDown(_axis, false, _lastInputValue);
                        CheckDoubleTap();
                    }
                    else
                    {
                        BroadcastInputDown(_axis, true, _lastInputValue);
                    }
                }
                else
                {
                    _pressed = false;
                    RemovePressedKey(this);
                    BroadcastInputUp(_axis);
                }
            }
        }

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
        public void AddOnNoPress(Action a) => Instance().OnNoPress += a;

        private static void BroadcastInputDown(InputAxis axis, bool isHeld, float lastInputValue)
        {
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                InputListeners[i].OnInputDown(axis, isHeld, lastInputValue);
            }
        }

        private static void BroadcastInputUp(InputAxis axis)
        {
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                InputListeners[i].OnInputUp(axis);
            }
        }

        private static void BroadCastDoubleTap(InputAxis axis, float lastInputValue)
        {
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                InputListeners[i].OnDoubleTap(axis, lastInputValue);
            }
        }

        public static void RegisterInputListener(IInputListener inputListener)
        {
            if (InputListeners.Contains(inputListener)) return;
            InputListeners.Add(inputListener);
        }

        public static void UnregisterInputListener(IInputListener inputListener)
        {
            InputListeners.Remove(inputListener);
        }
    }
}