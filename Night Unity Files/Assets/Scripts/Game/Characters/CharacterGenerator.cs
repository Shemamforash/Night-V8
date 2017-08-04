using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class CharacterGenerator
    {
        public static void GenerateCharacter()
        {

        }

        public static List<Character> LoadInitialParty()
        {
            List<Character> characters = new List<Character>();
            Character c = new Character("Driver", "Driver", 100, 100, 100, 100, 100, 100, Character.WeightCategory.Medium, 100, "Scavenger");
            characters.Add(c);
            c = new Character("Test", "Driver", 100, 100, 100, 100, 100, 100, Character.WeightCategory.Medium, 100, "Scavenger");
			characters.Add(c);
            return characters;
        }
    }
}
