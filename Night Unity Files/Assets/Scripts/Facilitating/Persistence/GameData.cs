using System.Collections.Generic;
using UnityEngine;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;

namespace Facilitating.Persistence
{
    public class GameData : MonoBehaviour
    {
        //Playthrough Settings
        public enum Difficulty { Easy, Normal, Hard };
        public static Difficulty DifficultySetting = Difficulty.Normal;
        public static bool PermadeathOn = true;

        //Options Settings
        public static float MasterVolume = 1;
        public static float MusicVolume = 1;
        public static float EffectsVolume = 1;
        public static List<Character> Party;
        
        //Camp Data
        public static int StoredWater = 0, StoredFood = 0, StoredFuel = 0;

        public static void SetDifficultyFromString(string difficultyString)
        {
            switch (difficultyString)
            {
                case "easy":
                    DifficultySetting = Difficulty.Easy;
                    break;
                case "normal":
                    DifficultySetting = Difficulty.Normal;
                    break;
                case "hard":
                    DifficultySetting = Difficulty.Hard;
                    break;
                default:
                    print("No Difficulty Selected");
                    break;
            }
        }

    }
}