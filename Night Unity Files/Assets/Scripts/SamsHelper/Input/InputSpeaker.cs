using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputSpeaker : MonoBehaviour
    {
        private readonly Dictionary<InputAxis, InputPress> _inputPressList = new Dictionary<InputAxis, InputPress>();
        private static float _lastInputValue;
        private static InputSpeaker _instance;

        public void Awake()
        {
            _inputPressList[InputAxis.Submit] = new InputPress("Submit");
            _inputPressList[InputAxis.Cancel] = new InputPress("Cancel");
            _inputPressList[InputAxis.Fire] = new InputPress("Fire");
            _inputPressList[InputAxis.Reload] = new InputPress("Reload");
            _instance = this;
        }

        public static InputSpeaker Instance()
        {
            if (_instance != null) return _instance;
            return FindObjectOfType<InputSpeaker>();
        }
        
        public void Update()
        {
            foreach (InputPress i in _inputPressList.Values)
            {
                i.CheckPress();
            }
        }

        public void AddOnPressEvent(InputAxis axis, Action a)
        {
            _inputPressList[axis].OnPress += a;
        }

        public void AddOnReleaseEvent(InputAxis axis, Action a)
        {
            _inputPressList[axis].OnRelease += a;
        }
        
        public void AddOnHoldEvent(InputAxis axis, Action a)
        {
            _inputPressList[axis].OnHold += a;
        }
        
        public void RemoveOnHoldEvent(Action onHold, InputAxis axis)
        {
            _inputPressList[axis].OnHold -= onHold;
        }
        
        private class InputPress
        {
            private bool _pressed;
            private readonly string _axisName;
            public event Action OnPress, OnRelease, OnHold;

            public InputPress(string axisName)
            {
                _axisName = axisName;
            }

            public void CheckPress()
            {
                _lastInputValue = UnityEngine.Input.GetAxis(_axisName);
                if (Math.Abs(_lastInputValue) > 0.0001f)
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        BroadcastPress();
                    }
                    BroadcastHeld();
                }
                else
                {
                    _pressed = false;
                    BroadcastRelease();
                }
            }
            
            private void BroadcastPress()
            {
                if (OnPress != null) OnPress();
            }

            private void BroadcastRelease()
            {
                if (OnRelease != null) OnRelease();
            }

            private void BroadcastHeld()
            {
                if (OnHold != null) OnHold();
            }
        }

        public static float LastInputValue()
        {
            return _lastInputValue;
        }
    }
}