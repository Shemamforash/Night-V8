using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Characters;

namespace Persistence
{
    public class GameData : MonoBehaviour
    {
        //Playthrough Settings
        public enum Difficulty { EASY, NORMAL, HARD };
        public static Difficulty difficultySetting = Difficulty.NORMAL;
        public static bool permadeathOn = true;

        //Options Settings
        public static float masterVolume = 1;
        public static float musicVolume = 1;
        public static float effectsVolume = 1;
        public static List<Character> party;
        
        //Camp Data
        public static float storedWater = 0f, storedFood = 0f, storedFuel = 0f;

        public static void SetDifficultyFromString(string difficultyString)
        {
            switch (difficultyString)
            {
                case "easy":
                    difficultySetting = GameData.Difficulty.EASY;
                    break;
                case "normal":
                    difficultySetting = GameData.Difficulty.NORMAL;
                    break;
                case "hard":
                    difficultySetting = GameData.Difficulty.HARD;
                    break;
                default:
                    print("No Difficulty Selected");
                    break;
            }
        }

    }
}