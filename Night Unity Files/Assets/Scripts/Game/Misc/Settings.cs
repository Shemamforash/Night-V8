using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
	//Playthrough Settings
    public enum Difficulty { EASY, NORMAL, HARD };
    public static Difficulty difficultySetting = Difficulty.NORMAL;
	public static bool permadeathOn = true;

	//Options Settings
	public static float masterVolume = 1;
	public static float musicVolume = 1;
	public static float effectsVolume = 1;

	public static void SetDifficultyFromString(string difficultyString){
		switch (difficultyString)
        {
            case "easy":
                difficultySetting = Settings.Difficulty.EASY;
                break;
            case "normal":
                difficultySetting = Settings.Difficulty.NORMAL;
                break;
            case "hard":
                difficultySetting = Settings.Difficulty.HARD;
                break;
            default:
                print("No Difficulty Selected");
                break;
        }
	}

}
