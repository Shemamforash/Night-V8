using System;
using System.Collections.Generic;
using System.Linq;
using InControl;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SamsHelper.Input
{
    public class InputHandler : MonoBehaviour
    {
        private InputActions _inputActions;
        private const float DoubleTapDuration = 0.3f;
        private readonly Dictionary<InputAxis, InputPress> _inputPressList = new Dictionary<InputAxis, InputPress>();
        private static readonly List<IInputListener> InputListeners = new List<IInputListener>();
        private static readonly List<IInputListener> ListenersToAdd = new List<IInputListener>();
        private static readonly List<IInputListener> ListenersToRemove = new List<IInputListener>();
        private static IInputListener _currentInputListener;
        private static bool _listenersInterrupted;
        private static InputHandler _instance;

        public void Awake()
        {
            _instance = this;
        }

        public static bool InputAxisWasPressed(InputAxis inputAxis) => _instance._inputPressList[inputAxis].WasPressed();

        public void Start()
        {
            _inputActions = new InputActions();
            _inputActions.BindActions();
            foreach (InputAxis axis in Enum.GetValues(typeof(InputAxis)))
            {
                InputPress inputPress;
                switch (axis)
                {
                    case InputAxis.Horizontal:
                        inputPress = new InputPress(axis, _inputActions.Horizontal);
                        break;
                    case InputAxis.Vertical:
                        inputPress = new InputPress(axis, _inputActions.Vertical);
                        break;
                    case InputAxis.Menu:
                        inputPress = new InputPress(axis, _inputActions.Menu);
                        break;
                    case InputAxis.Cancel:
                        inputPress = new InputPress(axis, _inputActions.Cancel);
                        break;
                    case InputAxis.Accept:
                        inputPress = new InputPress(axis, _inputActions.Accept);
                        break;
                    case InputAxis.SwitchTab:
                        inputPress = new InputPress(axis, _inputActions.ChangeTab);
                        break;
                    case InputAxis.Fire:
                        inputPress = new InputPress(axis, _inputActions.Fire);
                        break;
                    case InputAxis.Reload:
                        inputPress = new InputPress(axis, _inputActions.Reload);
                        break;
                    case InputAxis.Sprint:
                        inputPress = new InputPress(axis, _inputActions.Sprint);
                        break;
                    case InputAxis.SkillOne:
                        inputPress = new InputPress(axis, _inputActions.SkillOne);
                        break;
                    case InputAxis.SkillTwo:
                        inputPress = new InputPress(axis, _inputActions.SkillTwo);
                        break;
                    case InputAxis.SkillThree:
                        inputPress = new InputPress(axis, _inputActions.SkillThree);
                        break;
                    case InputAxis.SkillFour:
                        inputPress = new InputPress(axis, _inputActions.SkillFour);
                        break;
                    case InputAxis.Inventory:
                        inputPress = new InputPress(axis, _inputActions.Inventory);
                        break;
                    case InputAxis.Compass:
                        inputPress = new InputPress(axis, _inputActions.Compass);
                        break;
                    case InputAxis.TakeItem:
                        inputPress = new InputPress(axis, _inputActions.TakeItem);
                        break;
                    case InputAxis.Swivel:
                        inputPress = new InputPress(axis, _inputActions.Swivel);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _inputPressList[axis] = inputPress;
            }
        }

        public static bool BindInputModule(KeyboardInputModule inputModule)
        {
            if (_instance == null) return false;
            InputActions actions = _instance._inputActions;
            inputModule.SubmitAction = actions.Accept;
            inputModule.CancelAction = actions.Cancel;
            inputModule.MoveAction = actions.Move;
            return true;
        }

        private void OnDestroy()
        {
            InputListeners.Clear();
            ListenersToAdd.Clear();
            ListenersToRemove.Clear();
            _inputActions.Destroy();
            _instance = null;
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

        private static bool ListenerIsValid(IInputListener inputListener)
        {
            bool valid = true;
            switch (inputListener)
            {
                case MonoBehaviour m when m.gameObject == null:
                case null:
                    valid = false;
                    break;
            }

            if (valid) return true;
            ListenersToRemove.Add(inputListener);
            return false;
        }

        private static void BroadcastInputDown(InputAxis axis, bool isHeld, float lastInputValue)
        {
            if (ListenerIsValid(_currentInputListener)) _currentInputListener?.OnInputDown(axis, isHeld, lastInputValue);
            if (_listenersInterrupted) return;
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                if (InputListeners[i] == _currentInputListener) continue;
                if (!ListenerIsValid(InputListeners[i])) continue;
                InputListeners[i].OnInputDown(axis, isHeld, lastInputValue);
            }
        }

        private static void BroadcastInputUp(InputAxis axis)
        {
            if (ListenerIsValid(_currentInputListener)) _currentInputListener?.OnInputUp(axis);
            if (_listenersInterrupted) return;
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                if (InputListeners[i] == _currentInputListener) continue;
                if (!ListenerIsValid(InputListeners[i])) continue;
                InputListeners[i].OnInputUp(axis);
            }
        }

        private static void BroadCastDoubleTap(InputAxis axis, float lastInputValue)
        {
            if (ListenerIsValid(_currentInputListener)) _currentInputListener?.OnDoubleTap(axis, lastInputValue);
            if (_listenersInterrupted) return;
            for (int i = InputListeners.Count - 1; i >= 0; --i)
            {
                if (InputListeners[i] == _currentInputListener) continue;
                if (!ListenerIsValid(InputListeners[i])) continue;
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
            if (_currentInputListener == inputListener) _currentInputListener = null;
        }

        private class InputPress
        {
            private readonly InputAxis _axis;
            private float _directionAtLastPress;
            private float _lastInputValue, _currentInputValue;
            private bool _held;
            private float _timeAtLastPress;
            private readonly PlayerAction _playerAction;
            private readonly PlayerOneAxisAction _playerAxis;

            public InputPress(InputAxis axis, PlayerAction playerAction)
            {
                _axis = axis;
                _playerAction = playerAction;
            }

            public InputPress(InputAxis axis, PlayerOneAxisAction inputAxis)
            {
                _axis = axis;
                _playerAxis = inputAxis;
            }

            private void CheckDoubleTap()
            {
                float currentTime = Helper.TimeInSeconds();
                float timeBetweenClicks = currentTime - _timeAtLastPress;
                if (timeBetweenClicks < DoubleTapDuration && _directionAtLastPress.HasSameSignAs(_currentInputValue))
                {
                    BroadCastDoubleTap(_axis, _directionAtLastPress);
                    currentTime = 0;
                }

                _directionAtLastPress = _currentInputValue < 0 ? -1 : 1;
                _timeAtLastPress = currentTime;
            }

            public void CheckPress()
            {
                _currentInputValue = _playerAxis?.Value ?? _playerAction.Value;
                bool isPressed = _currentInputValue != 0f;
                if (isPressed)
                {
                    _held = _currentInputValue.HasSameSignAs(_lastInputValue);
                    BroadcastInputDown(_axis, _held, _currentInputValue); //.Polarity());
                    if (!_held) CheckDoubleTap();
                }
                else if (_lastInputValue != 0f)
                {
                    _held = false;
                    BroadcastInputUp(_axis);
                }

                _lastInputValue = _currentInputValue;
            }

            public bool WasPressed()
            {
                return _playerAction?.IsPressed ?? _playerAxis.IsPressed;
            }
        }

        public static void InterruptListeners(bool interrupted)
        {
            _listenersInterrupted = interrupted;
        }
    }
}