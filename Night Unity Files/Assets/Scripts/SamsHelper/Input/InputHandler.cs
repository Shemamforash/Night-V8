﻿using System;
using System.Collections.Generic;
using System.Reflection;
using SamsHelper.Libraries;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputHandler : MonoBehaviour
    {
        private const float DoubleTapDuration = 0.3f;
        private readonly Dictionary<InputAxis, InputPress> _inputPressList = new Dictionary<InputAxis, InputPress>();
        private static readonly List<IInputListener> InputListeners = new List<IInputListener>();
        private static readonly List<IInputListener> ListenersToAdd = new List<IInputListener>();
        private static readonly List<IInputListener> ListenersToRemove = new List<IInputListener>();
        private static IInputListener _currentInputListener;
        private static bool _listenersInterrupted;

        public void Awake()
        {
            foreach (InputAxis axis in Enum.GetValues(typeof(InputAxis))) AddInputPress(axis);
        }

        private void AddInputPress(InputAxis axis)
        {
            _inputPressList[axis] = new InputPress(axis);
        }

        private void OnDestroy()
        {
            InputListeners.Clear();
            ListenersToAdd.Clear();
            ListenersToRemove.Clear();
        }

        public void Update()
        {
            if (ListenersToAdd.Count != 0)
            {
                InputListeners.AddRange(ListenersToAdd);
                ListenersToAdd.Clear();
            }

            if (ListenersToRemove.Count != 0)
            {
                InputListeners.RemoveAll(r => ListenersToRemove.Contains(r));
                ListenersToRemove.Clear();
            }

            foreach (InputPress i in _inputPressList.Values) i.CheckPress();
        }

        private static void BroadcastInputDown(InputAxis axis, bool isHeld, float lastInputValue)
        {
            _currentInputListener?.OnInputDown(axis, isHeld, lastInputValue);
            if (_listenersInterrupted) return;
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                if (InputListeners[i] == _currentInputListener) continue;
                InputListeners[i].OnInputDown(axis, isHeld, lastInputValue);
            }
        }

        private static void BroadcastInputUp(InputAxis axis)
        {
            _currentInputListener?.OnInputUp(axis);
            if (_listenersInterrupted) return;
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                if (InputListeners[i] == _currentInputListener) continue;
                InputListeners[i].OnInputUp(axis);
            }
        }

        private static void BroadCastDoubleTap(InputAxis axis, float lastInputValue)
        {
            _currentInputListener?.OnDoubleTap(axis, lastInputValue);
            if (_listenersInterrupted) return;
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                if (InputListeners[i] == _currentInputListener) continue;
                InputListeners[i].OnDoubleTap(axis, lastInputValue);
            }
        }

        public static void SetCurrentListener(IInputListener inputListener)
        {
            _currentInputListener = inputListener;
        }

        public static IInputListener GetCurrentListener()
        {
            return _currentInputListener;
        }

        public static void RegisterInputListener(IInputListener inputListener)
        {
            if (ListenersToRemove.Contains(inputListener)) ListenersToRemove.Remove(inputListener);
            if (InputListeners.Contains(inputListener)) return;
            ListenersToAdd.Add(inputListener);
        }

        public static void UnregisterInputListener(IInputListener inputListener)
        {
            if (ListenersToAdd.Contains(inputListener)) ListenersToAdd.Remove(inputListener);
            ListenersToRemove.Add(inputListener);
        }

        private class InputPress
        {
            private readonly InputAxis _axis;
            private readonly string _axisString;
            private float _directionAtLastPress;
            private float _lastInputValue, _currentInputValue;
            private bool _held;
            private float _timeAtLastPress;

            public InputPress(InputAxis axis)
            {
                _axis = axis;
                _axisString = _axis.ToString();
            }

            private void CheckDoubleTap()
            {
                float currentTime = Helper.TimeInSeconds();
                float timeBetweenClicks = currentTime - _timeAtLastPress;
                if (timeBetweenClicks < DoubleTapDuration && Helper.ValuesHaveSameSign(_directionAtLastPress, _currentInputValue))
                {
                    BroadCastDoubleTap(_axis, _directionAtLastPress);
                    currentTime = 0;
                }

                _directionAtLastPress = _currentInputValue < 0 ? -1 : 1;
                _timeAtLastPress = currentTime;
            }

            public void CheckPress()
            {
                if (_axis == InputAxis.Mouse && Cursor.visible == false) return;
                _currentInputValue = UnityEngine.Input.GetAxisRaw(_axisString);
                bool isPressed = _currentInputValue != 0f;
                if (isPressed)
                {
                    _held = Helper.ValuesHaveSameSign(_currentInputValue, _lastInputValue);
                    BroadcastInputDown(_axis, _held, _currentInputValue.Polarity());
                    if (!_held) CheckDoubleTap();
                }
                else if (_lastInputValue != 0f)
                {
                    _held = false;
                    BroadcastInputUp(_axis);
                }

                _lastInputValue = _currentInputValue;
            }
        }

        public static void InterruptListeners(bool interrupted)
        {
            _listenersInterrupted = interrupted;
        }
    }
}