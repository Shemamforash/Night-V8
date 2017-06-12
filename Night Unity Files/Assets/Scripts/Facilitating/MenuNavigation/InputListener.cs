using System;
using System.Collections.Generic;

namespace Menus
{
    public class InputListener
    {
        private List<Action> cancelActions = new List<Action>();
        private List<Action> confirmActions = new List<Action>();

        public InputListener()
        {
            InputSpeaker.RegisterForInput(this);
        }

        public void ReceiveCancelEvent()
        {
            cancelActions.ForEach(a => a());
        }

        public void ReceiveConfirmEvent()
        {
            confirmActions.ForEach(a => a());
        }

        public void OnCancel(Action cancelAction)
        {
            cancelActions.Add(cancelAction);
        }

        public void OnConfirm(Action confirmAction)
        {
            confirmActions.Add(confirmAction);
        }
    }
}