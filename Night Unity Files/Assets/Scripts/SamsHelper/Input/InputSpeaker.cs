using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper.Input
{
    public class InputSpeaker : MonoBehaviour
    {
        private static readonly List<InputListener> Listeners = new List<InputListener>();
        private readonly List<InputPress> _inputPressList = new List<InputPress>();
        private static float _lastInputValue;

        public void Awake()
        {
            _inputPressList.Add(new InputPress("Submit", InputAxis.Submit));
            _inputPressList.Add(new InputPress("Cancel", InputAxis.Cancel));
            _inputPressList.Add(new InputPress("Fire", InputAxis.Fire));
            _inputPressList.Add(new InputPress("Reload", InputAxis.Reload));
        }

        public void Update()
        {
            foreach (InputPress i in _inputPressList)
            {
                i.CheckPress();
            }
        }
        
        public static void RegisterForInput(InputListener listener)
        {
            Listeners.Add(listener);
        }

        private class InputPress
        {
            private bool _pressed;
            private readonly string _axisName;
            private readonly InputAxis _inputAxis;

            public InputPress(string axisName, InputAxis inputAxis)
            {
                _axisName = axisName;
                _inputAxis = inputAxis;
            }

            public void CheckPress()
            {
                _lastInputValue = UnityEngine.Input.GetAxis(_axisName);
                if (_lastInputValue != 0)
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        BroadcastPress(_inputAxis);
                    }
                }
                else
                {
                    _pressed = false;
                    BroadcastRelease(_inputAxis);
                }
            }
        }

        public static float LastInputValue()
        {
            return _lastInputValue;
        }

        private static void BroadcastPress(InputAxis axis)
        {
            Listeners.ForEach(l => l.ReceivePressEvent(axis));
        }

        private static void BroadcastRelease(InputAxis axis)
        {
            Listeners.ForEach(l => l.ReceiveReleaseEvent(axis));
        }
    }
}