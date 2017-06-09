using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeListener : MonoBehaviour {
	public void Subscribe(){
		WorldTime.SubscribeTimeListener(this);
	}

	public abstract void ReceiveHourEvent();
	public abstract void ReceiveDayEvent();
	public abstract void ReceivePauseEvent(bool paused);
}
