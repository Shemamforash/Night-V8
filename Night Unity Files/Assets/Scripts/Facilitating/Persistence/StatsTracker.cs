using UnityEngine;
using System;
using UnityEngine.UI;

public class StatsTracker : MonoBehaviour {
	private static float sessionTime = 0;
	public Text statsText;

	void Update () {
		sessionTime += Time.deltaTime;
		TimeSpan t = TimeSpan.FromSeconds(sessionTime);
		string statString = "Session Play Time: " + t.Hours + "hours " + t.Minutes + "mins " +  t.Seconds + "secs";
		statsText.text = statString;
	}
}
