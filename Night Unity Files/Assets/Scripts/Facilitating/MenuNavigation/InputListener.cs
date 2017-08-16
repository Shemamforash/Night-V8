using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;

namespace Menus
{
    public class InputListener
    {
        private readonly InputTypeActionList _pressActions = new InputTypeActionList();
        private readonly InputTypeActionList _releaseActions = new InputTypeActionList();

        public InputListener()
        {
            InputSpeaker.RegisterForInput(this);
        }

        private class InputTypeActionList
        {
            private readonly Dictionary<InputAxis, List<Action>> _actions = new Dictionary<InputAxis, List<Action>>();

            public void Subscribe(InputAxis inputAxis, Action action)
            {
                if (_actions.ContainsKey(inputAxis))
                {
                    _actions[inputAxis].Add(action);
                }
                else
                {
                    _actions[inputAxis] = new List<Action> {action};
                }
            }

            public void ReceiveEvent(InputAxis inputAxis)
            {
                List<Action> actionList;
                if (_actions.TryGetValue(inputAxis, out actionList))
                {
                    actionList.ForEach(a => a());
                }
            }
        }

        public void ReceivePressEvent(InputAxis inputAxis)
        {
            _pressActions.ReceiveEvent(inputAxis);
        }

        public void ReceiveReleaseEvent(InputAxis inputAxis)
        {
            _releaseActions.ReceiveEvent(inputAxis);
        }

        public void OnAxisPress(InputAxis inputAxis, Action action)
        {
            _pressActions.Subscribe(inputAxis, action);
        }

        public void OnAxisRelease(InputAxis inputAxis, Action action)
        {
            _releaseActions.Subscribe(inputAxis, action);
        }
    }
}