using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PermadeathToggler : Toggler {
	private static bool permadeathOn = true;

	protected override void On(){
		permadeathOn = true;
	}

	protected override void Off(){
		permadeathOn = false;
	}

	public static bool IsPermadeathOn(){
		return permadeathOn;
	}
}
