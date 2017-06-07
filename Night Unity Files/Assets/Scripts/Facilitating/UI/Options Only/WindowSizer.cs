using UnityEngine;

public class WindowSizer : Toggler
{
    protected override void On()
    {
        base.On();
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
    }

    protected override void Off()
    {
        base.Off();
        Screen.SetResolution((int)(Screen.currentResolution.width * 0.75f), (int)(Screen.currentResolution.height * 0.75f), false);
    }
}
