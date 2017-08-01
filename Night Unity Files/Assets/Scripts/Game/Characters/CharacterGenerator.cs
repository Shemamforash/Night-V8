using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters
{
    public class CharacterGenerator : MonoBehaviour
    {
        public static void GenerateCharacter()
        {

        }

		public static List<Character> LoadInitialParty(){
			List<Character> characters = new List<Character>();
			Character c = new Character("Driver", "Driver", 100, 100, 100, 100, 100, 100, Character.WeightCategory.MEDIUM, 100, Traits.FindTrait("Nomadic"), Traits.FindTrait("Scavenger"));
			Debug.Log(c);
			characters.Add(c);
			return characters;
		}
    }
}
