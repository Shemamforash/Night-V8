using System;
using System.Collections.Generic;
using System.Linq;
using Game.Combat.CombatStates;
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
        private event Action OnNoPress;
        private static readonly List<IInputListener> InputListeners = new List<IInputListener>();


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

        private class InputPress
        {
            private bool _pressed;
            private readonly InputAxis _axis;

            public InputPress(InputAxis axis)
            {
                _axis = axis;
            }

            public void CheckPress()
            {
                _lastInputValue = UnityEngine.Input.GetAxisRaw(_axis.ToString());
                if (Math.Abs(_lastInputValue) > 0.0001f)
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        AddPressedKey(this);
                        BroadcastInputDown(_axis, false);
                    }
                    else
                    {
                        BroadcastInputDown(_axis, true);
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
        public void AddOnNoPress(Action a) => Instance().OnNoPress += a;

        private static void BroadcastInputDown(InputAxis axis, bool isHeld)
        {
            InputListeners.ForEach(l => l.OnInputDown(axis, isHeld, _lastInputValue));
        }

        private static void BroadcastInputUp(InputAxis axis)
        {
            InputListeners.ForEach(l => l.OnInputUp(axis));
        }

        public static void RegisterInputListener(IInputListener inputListener)
        {
            if (InputListeners.Contains(inputListener)) return;
            InputListeners.Add(inputListener);
        }
    }
}