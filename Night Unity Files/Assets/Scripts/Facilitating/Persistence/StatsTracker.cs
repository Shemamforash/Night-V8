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
		statString += "\nDeaths: 0";
		statString += "\nWeapons found: 0";
		statString += "\nEnemies eaten: 0";
		statString += "\nSurvivors lost: 0";
		statString += "\nBullets wasted: 0";
		statString += "\nRemnants found: 0";
		statString += "\nTears spilled: 0";
		statString += "\nKilometres travelled: 0";
		statsText.text = statString;
	}
}
