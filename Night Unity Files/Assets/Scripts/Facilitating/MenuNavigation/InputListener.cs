using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;

namespace Menus
{
    public class InputListener
    {
        private readonly List<Action> _cancelActions = new List<Action>();
        private readonly List<Action> _confirmActions = new List<Action>();
        private readonly List<Action> _fireActions = new List<Action>();
        private readonly List<Action> _reloadActions = new List<Action>();

        public InputListener()
        {
            InputSpeaker.RegisterForInput(this);
        }

        public void ReceiveInputEvent(InputAxis inputAxis)
        {
            switch (inputAxis)
            {
                case InputAxis.Cancel:
                    _cancelActions.ForEach(a => a());
                    break;
                case InputAxis.Submit:
                    _confirmActions.ForEach(a => a());
                    break;
                case InputAxis.Fire:
                    _fireActions.ForEach(a => a());
                    break;
                case InputAxis.Reload:
                    _reloadActions.ForEach(a => a());

                    break;
            }
        }

        public void OnAxis(InputAxis inputAxis, Action action)
        {
            switch (inputAxis)
            {
                case InputAxis.Cancel:
                    _cancelActions.Add(action);
                    break;
                case InputAxis.Submit:
                    _confirmActions.Add(action);
                    break;
                case InputAxis.Fire:
                    _fireActions.Add(action);
                    break;
                case InputAxis.Reload:
                    _reloadActions.Add(action);
                    break;
            }
        }
    }
}