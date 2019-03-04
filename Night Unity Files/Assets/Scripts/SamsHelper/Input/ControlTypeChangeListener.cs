using System;
using UnityEngine;

namespace SamsHelper.Input
{
    public class ControlTypeChangeListener : MonoBehaviour
    {
        private Action _onControllerInputChange;

        public void OnEnable()
        {
            InputHandler.RegisterControlTypeChangeListener(this);
        }

        private void OnDisable()
        {
            InputHandler.UnregisterControlTypeChangeListener(this);
        }

        public void SetOnControllerInputChange(Action onControllerInputChange)
        {
            _onControllerInputChange = onControllerInputChange;
            Execute();
        }

        public void Execute()
        {
            _onControllerInputChange?.Invoke();
        }
    }
}