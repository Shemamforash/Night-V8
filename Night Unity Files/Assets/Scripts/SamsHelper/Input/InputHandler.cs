﻿using System;
using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputHandler : MonoBehaviour
    {
        private const float _doubleTapDuration = 300;
        private readonly Dictionary<InputAxis, InputPress> _inputPressList = new Dictionary<InputAxis, InputPress>();
        private static readonly List<IInputListener> InputListeners = new List<IInputListener>();
        private static readonly List<IInputListener> ListenersToAdd = new List<IInputListener>();
        private static readonly List<IInputListener> ListenersToRemove = new List<IInputListener>();
        private static IInputListener _inputListener;

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
            _inputListener?.OnInputDown(axis, isHeld, lastInputValue);
            for (int i = InputListeners.Count - 1; i >= 0; --i) InputListeners[i].OnInputDown(axis, isHeld, lastInputValue);
        }

        private static void BroadcastInputUp(InputAxis axis)
        {
            _inputListener?.OnInputUp(axis);

            for (int i = InputListeners.Count - 1; i >= 0; --i) InputListeners[i].OnInputUp(axis);
        }

        private static void BroadCastDoubleTap(InputAxis axis, float lastInputValue)
        {
            _inputListener?.OnDoubleTap(axis, lastInputValue);

            for (int i = InputListeners.Count - 1; i >= 0; --i) InputListeners[i].OnDoubleTap(axis, lastInputValue);
        }

        public static void SetCurrentListener(IInputListener inputListener)
        {
            _inputListener = inputListener;
        }

        public static void RegisterInputListener(IInputListener inputListener)
        {
            if (InputListeners.Contains(inputListener)) return;
            ListenersToAdd.Add(inputListener);
        }

        public static void UnregisterInputListener(IInputListener inputListener)
        {
            ListenersToRemove.Add(inputListener);
        }

        private class InputPress
        {
            private readonly InputAxis _axis;
            private float _directionAtLastPress;
            private float _lastInputValue;
            private bool _pressed;
            private long _timeAtLastPress;

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
                float currentInputValue = UnityEngine.Input.GetAxis(_axis.ToString());
                if (Math.Abs(currentInputValue) > 0f)
                {
                    _pressed = Helper.ValuesHaveSameSign(currentInputValue, _lastInputValue);
                    BroadcastInputDown(_axis, _pressed, currentInputValue);
                    CheckDoubleTap();
                }
                else
                {
                    _pressed = false;
                    BroadcastInputUp(_axis);
                }

                _lastInputValue = currentInputValue;
            }
        }
    }
}