using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermadeathToggler : Toggler
{
    protected override void On()
    {
        Settings.permadeathOn = true;
    }

    protected override void Off()
    {
        Settings.permadeathOn = false;
    }
}
