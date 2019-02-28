using System;
using InControl;
using SamsHelper.Input;
using UnityEngine.Serialization;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event/Standalone Input Module")]
    public class KeyboardInputModule : InControlInputModule
    {
        private const float StopMouseInputTimeMax = 2f;
        private float _timeSinceLastInput;
        private static bool _isMouseInputAccepted;
        private static Vector2 _lastMousePosition;
        private bool _boundInput;

        public override void Update()
        {
            if (!_boundInput) _boundInput = InputHandler.BindInputModule(this);
            base.Update();
            Cursor.lockState = CursorLockMode.Confined;
            Vector2 currentMousePosition = Input.mousePosition;
            bool mousePressed = Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Mouse2);
            if (currentMousePosition == _lastMousePosition && !mousePressed)
            {
                if (_timeSinceLastInput >= StopMouseInputTimeMax)
                {
                    _isMouseInputAccepted = false;
                    return;
                }

                _timeSinceLastInput += Time.unscaledDeltaTime;
                return;
            }

            _timeSinceLastInput = 0f;
            _lastMousePosition = currentMousePosition;
            _isMouseInputAccepted = true;
        }

        public override void Process()
        {
            base.Process();
            Cursor.visible = _isMouseInputAccepted;
        }
    }
}