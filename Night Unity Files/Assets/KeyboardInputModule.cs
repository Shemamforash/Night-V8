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

        Cursor.visible = !CameraLock.IsCameraLocked();
        if (Cursor.visible) ProcessMouseEvent();
    }
}