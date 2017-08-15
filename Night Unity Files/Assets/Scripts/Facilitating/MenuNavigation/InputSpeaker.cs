using System.Collections.Generic;
using Menus;
using UnityEngine;

namespace Facilitating.MenuNavigation
{
    public partial class InputSpeaker : MonoBehaviour
    {
        private static readonly List<InputListener> Listeners = new List<InputListener>();
        private readonly List<InputPress> _inputPressList = new List<InputPress>();

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
                if (Input.GetAxis(_axisName) != 0)
                {
                    if (!_pressed)
                    {
                        _pressed = true;
                        Broadcast(_inputAxis);
                    }
                }
                else
                {
                    _pressed = false;
                }
            }
        }

        private static void Broadcast(InputAxis axis)
        {
            foreach (InputListener l in Listeners)
            {
                l.ReceiveInputEvent(axis);
            }
        }
    }
}