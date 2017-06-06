using UnityEngine;

public class WindowSizer : Toggler
{
    protected override void On()
    {
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
    }

    protected override void Off()
    {
        Screen.SetResolution((int)(Screen.currentResolution.width * 0.75f), (int)(Screen.currentResolution.height * 0.75f), false);
    }
}
