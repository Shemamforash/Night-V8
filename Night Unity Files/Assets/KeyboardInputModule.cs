using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardInputModule : StandaloneInputModule
{
    public override void Process()
    {
        bool usedEvent = SendUpdateEventToSelectedObject();
        if (eventSystem.sendNavigationEvents)
        {
            if (!usedEvent) usedEvent = SendMoveEventToSelectedObject();
            if (!usedEvent) SendSubmitEventToSelectedObject();
        }

        if (Cursor.visible) ProcessMouseEvent();
    }
}